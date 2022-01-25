using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notion.Client;
using NotionFinance.Data;
using NotionFinance.Exceptions;
using NotionFinance.Models;
using NotionFinance.Models.Tables;
using NotionFinance.Services.Forex;
using Polly;
using Serilog;
using User = NotionFinance.Models.User;

namespace NotionFinance.Services;

public class NotionAutoUpdateService : BackgroundService
{
    private ILogger<NotionAutoUpdateService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private DateTime LastForexUpdate { get; set; } = DateTime.MinValue;

    public NotionAutoUpdateService(IServiceProvider serviceProvider, ILogger<NotionAutoUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var cryptocurrencyService = scope.ServiceProvider.GetRequiredService<ICryptocurrencyService>();
        var forexService = scope.ServiceProvider.GetRequiredService<IForexService>();
        var forexServices = scope.ServiceProvider.GetServices<IForexService>();
        var userDbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        await userDbContext.Database.MigrateAsync(stoppingToken);
        while (!stoppingToken.IsCancellationRequested)
        {
            var users = await userDbContext.Users
                .Include(x => x.NotionUserSettings)
                .Where(x => x.NotionUserSettings.NotionAccessToken != null)
                .ToListAsync(stoppingToken);
            var options = new ParallelOptions
            {
                CancellationToken = stoppingToken,
                MaxDegreeOfParallelism = System.Environment.ProcessorCount
            };
            await Parallel.ForEachAsync(users, options, async (user, cancellationToken) =>
            {
                try
                {
                    _logger.LogInformation("Starting update process for user {UserId}", user.Id);
                    var notionService = new NotionService(userDbContext,
                        NotionClientFactory.Create(new ClientOptions()
                            {AuthToken = user.NotionUserSettings.NotionAccessToken}));
                    MasterTable? masterTable;
                    try
                    {
                        if (!user.NotionUserSettings.MasterDatabaseExists)
                        {
                            try
                            {
                                if (user.NotionUserSettings.MasterDatabaseId == null)
                                {
                                    var masterDbName = user.NotionUserSettings.MasterDatabaseName ?? "Master Database";
                                    var masterDb = await notionService.GetDatabaseByNameAsync(masterDbName);
                                    user.NotionUserSettings.MasterDatabaseExists = true;
                                    await userDbContext.SaveChangesAsync(cancellationToken);
                                    _logger.LogDebug("Found Master Database for new user, will update in next cycle");
                                    return;
                                }
                                else
                                {
                                    var masterDb =
                                        await notionService.GetDatabaseByIdAsync(user.NotionUserSettings
                                            .MasterDatabaseId);
                                    user.NotionUserSettings.MasterDatabaseExists = true;
                                    await userDbContext.SaveChangesAsync(cancellationToken);
                                    _logger.LogDebug("Found Master Database for new user, will update in next cycle");
                                    return;
                                }
                            }
                            catch (NotionDatabaseNotFoundException e)
                            {
                                // Ignored
                            }

                            _logger.LogError("Master database does not exist for User {Id}, creating now", user.Id);
                            var db = await notionService.CreateMasterTable();
                            user.NotionUserSettings.MasterDatabaseExists = true;
                            user.NotionUserSettings.MasterDatabaseId = db.Id;
                            user.NotionUserSettings.MasterDatabaseFetchingFailedCount = 0;
                            user.NotionUserSettings.MasterDatabaseCreationTime = DateTime.Now;
                            await userDbContext.SaveChangesAsync(cancellationToken);
                            return;
                        }

                        await Policy
                            .Handle<NotionDatabaseNotFoundException>()
                            .WaitAndRetryAsync(10,
                                retryAttempt => TimeSpan.FromSeconds(Math.Pow(retryAttempt, 1.45)),
                                (exception, span, retryCount, context) =>
                                    _logger.LogDebug(
                                        "Retry attempt {Count} failed with exception {Message}, trying after {Span} seconds",
                                        retryCount, exception.Message, span.Seconds))
                            .ExecuteAsync(async () =>
                            {
                                var masterDb =
                                    await notionService.GetDatabaseByIdAsync(user.NotionUserSettings.MasterDatabaseId!);
                                _logger.LogInformation("Master Database found for User {Id}", user.Id);
                                var masterDbPages = await notionService.GetPagesByDatabaseAsync(masterDb.Id);
                                masterTable = await MasterTable.Create(masterDb, masterDbPages);
                                await UpdateMasterDatabaseForUser(notionService, cryptocurrencyService, masterTable);
                                LastForexUpdate =
                                    await UpdateForexDataForMasterDatabase(notionService, forexService, masterTable,
                                        LastForexUpdate);
                            });
                    }
                    catch (NotionDatabaseNotFoundException e)
                    {
                        if (!user.NotionUserSettings.MasterDatabaseExists ||
                            user.NotionUserSettings.MasterDatabaseFetchingFailedCount >= 3)
                        {
                            _logger.LogError(e, "Master database not found for User {Id}, creating", user.Id);
                            await notionService.CreateMasterTable();
                            user.NotionUserSettings.MasterDatabaseExists = true;
                            user.NotionUserSettings.MasterDatabaseFetchingFailedCount = 0;
                            user.NotionUserSettings.MasterDatabaseCreationTime = DateTime.Now;
                            await userDbContext.SaveChangesAsync(cancellationToken);
                            return;
                        }

                        user.NotionUserSettings.MasterDatabaseFetchingFailedCount += 1;
                        await userDbContext.SaveChangesAsync(cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e, "");
                }
            });
            await Task.Delay(5_000, stoppingToken);
        }
    }

    private static async Task<DateTime> UpdateForexDataForMasterDatabase(INotionService notionService,
        IForexService forexService,
        MasterTable masterTable, DateTime lastForexUpdate)
    {
        if (DateTime.Now - lastForexUpdate < TimeSpan.FromMinutes(5)) return lastForexUpdate;
        foreach (var page in masterTable.ForexPages)
        {
            Currency from;
            Currency to;
            try
            {
                if (page.Ticker == null) continue;
                from = new Currency() {Ticker = page.Ticker[..3], Value = 1};
                to = new Currency() {Ticker = page.Ticker[3..], Value = 1};
            }
            catch (Exception e)
            {
                Log.Debug(e, "Failed to get currency tickers");
                continue;
            }

            var conversion = await forexService.ConvertAsync(from, to);

            await notionService.UpdatePageAsync(page.NotionPage, new PagesUpdateParameters()
            {
                Archived = false,
                Properties = new Dictionary<string, PropertyValue>()
                {
                    {
                        "Current Price",
                        new NumberPropertyValue() {Number = conversion.Rates[0].Value}
                    }
                }
            });
        }

        return DateTime.Now;
    }

    private static async Task UpdateMasterDatabaseForUser(INotionService
        notionService, ICryptocurrencyService cryptocurrencyService, MasterTable masterTable)
    {
        var coins = (await cryptocurrencyService.GetAllSupportedCryptocurrenciesAsync()).ToList();
        foreach (var page in masterTable.TokenPages)
        {
            try
            {
                if (page.Ticker == null) continue;
                Cryptocurrency j;
                try
                {
                    // TODO Change this to use cryptocurrency name also
                    j = coins.First(x => x.Symbol == page.Ticker.ToLower());
                }
                catch (InvalidOperationException e)
                {
                    continue;
                }

                var k = await cryptocurrencyService.GetCryptocurrencyDetailsAsync(j);
                // TODO If error: change ticker value to Error Code with current ticker itself

                await notionService.UpdatePageAsync(page.NotionPage, new PagesUpdateParameters()
                {
                    Archived = false,
                    Properties = new Dictionary<string, PropertyValue>()
                    {
                        {
                            "Current Price",
                            new NumberPropertyValue() {Number = k.CurrentPrice}
                        }
                    }
                });
            }
            catch (Exception e)
            {
                Log.Information(e, "");
            }
        }
    }
}