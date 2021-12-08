using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notion.Client;
using NotionFinance.Data;
using NotionFinance.Exceptions;
using NotionFinance.Models;
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
            foreach (var user in await userDbContext.Users.Where(x => x.NotionAccessToken != null).ToListAsync(stoppingToken))
            {
                _logger.LogInformation("Starting update process for user {UserId}", user.Id);
                var notionService = new NotionService(userDbContext,
                    NotionClientFactory.Create(new ClientOptions() {AuthToken = user.NotionAccessToken}));
                Database masterDb;
                IEnumerable<Page> masterDbPages;
                try
                {
                    masterDb = await notionService.GetDatabaseByNameAsync("Master Database");
                    masterDbPages = await notionService.GetPagesByDatabaseAsync(masterDb.Id);
                    _logger.LogInformation("Master Database found for User {Id}", user.Id);
                }
                catch (NotionDatabaseNotFoundException e)
                {
                    _logger.LogError(e, "Master database not found");
                    continue;
                }

                var tokenPages = new List<Page>();
                foreach (var page in masterDbPages)
                {
                    try
                    {
                        var typePropertyValue = page.Properties.First(x => x.Key == "Type").Value;
                        if (typePropertyValue == null) continue;
                        var selectName = (typePropertyValue as SelectPropertyValue)!.Select.Name;
                        if (selectName == "Token") tokenPages.Add(page);
                    }
                    catch (Exception e)
                    {
                        // ignored
                        _logger.LogInformation("Couldn't get TYPE column from master database");
                    }
                }

                var coins = (await cryptocurrencyService.GetAllSupportedCryptocurrenciesAsync()).ToList();
                foreach (var page in tokenPages)
                {
                    try
                    {
                        var tickerPropertyValue = page.Properties.First(x => x.Key == "Ticker").Value;
                        if (tickerPropertyValue == null)
                        {
                            continue;
                        }

                        var ticker = (tickerPropertyValue as RichTextPropertyValue)!.RichText[0].PlainText;

                        var namePropertyValue = page.Properties.First(x => x.Key == "Name").Value;
                        if (namePropertyValue == null)
                        {
                            continue;
                        }

                        var name = (namePropertyValue as TitlePropertyValue)!.Title[0].PlainText;
                        Cryptocurrency j;
                        try
                        {
                            // TODO Change this to use cryptocurrency name also
                            j = coins.First(x =>
                                x.Symbol == ticker.ToLower() && (name.ToLower().Contains(x.Name!.ToLower())));
                        }
                        catch (InvalidOperationException e)
                        {
                            continue;
                        }

                        var k = await cryptocurrencyService.GetCryptocurrencyDetailsAsync(j);
                        // TODO If error: change ticker value to Error Code with current ticker itself
                        var currentPricePropertyValue = page.Properties.First(x => x.Key == "Current Price").Value;
                        if (currentPricePropertyValue == null)
                        {
                            continue;
                        }

                        await notionService.UpdatePageAsync(page, new PagesUpdateParameters()
                        {
                            Archived = false,
                            Properties = new Dictionary<string, PropertyValue>()
                            {
                                {
                                    "Current Price",
                                    new NumberPropertyValue()
                                        {Id = currentPricePropertyValue.Id, Number = k.CurrentPrice}
                                }
                            }
                        });
                        _logger.LogInformation("Master database updated for user {UserId}", user.Id);
                    }
                    catch (Exception e)
                    {
                        // Ignored
                    }
                }

            }

            await Task.Delay(5_000);
        }
    }

    private async Task UpdateMasterDatabaseForUser(INotionService
        notionService, ICryptocurrencyService cryptocurrencyService)
    {
    }
}