using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notion.Client;
using NotionFinance.Data;
using NotionFinance.Exceptions;
using NotionFinance.Models;
using NotionFinance.Models.Tables;
using Serilog;
using User = NotionFinance.Models.User;

namespace NotionFinance.Services;

public class NotionAutoUpdateService : BackgroundService
{
    private ILogger<NotionAutoUpdateService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public NotionAutoUpdateService(IServiceProvider serviceProvider, ILogger<NotionAutoUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var cryptocurrencyService = scope.ServiceProvider.GetRequiredService<ICryptocurrencyService>();
        var userDbContext = scope.ServiceProvider.GetRequiredService<UserDbContext>();
        while (!stoppingToken.IsCancellationRequested)
        {
            foreach (var user in await userDbContext.Users.Where(x => x.NotionAccessToken != null)
                         .ToListAsync(stoppingToken))
            {
                _logger.LogInformation("Starting update process for user {UserId}", user.Id);
                var notionService = new NotionService(userDbContext,
                    NotionClientFactory.Create(new ClientOptions() {AuthToken = user.NotionAccessToken}));
                MasterTable masterTable;
                try
                {
                    var masterDb = await notionService.GetDatabaseByNameAsync("Master Database");
                    var masterDbPages = await notionService.GetPagesByDatabaseAsync(masterDb.Id);
                    masterTable = await MasterTable.Create(masterDb, masterDbPages);
                    _logger.LogInformation("Master Database found for User {Id}", user.Id);
                }
                catch (NotionDatabaseNotFoundException e)
                {
                    _logger.LogError(e, "Master database not found");
                    continue;
                }

                await UpdateMasterDatabaseForUser(notionService, cryptocurrencyService, masterTable);
            }

            await Task.Delay(5_000, stoppingToken);
        }
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