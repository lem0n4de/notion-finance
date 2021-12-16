using Notion.Client;
using NotionFinance.Models;

namespace NotionFinance.Services.Forex;

public interface IForexService
{
    public Task<CurrencyConversion> ConvertAsync(Currency from, Currency to);
    public Task<CurrencyConversion> ConvertAsync(Currency from, List<Currency> currencies);
    public Task<CurrencyConversion> ConvertOnDateAsync(Currency from, Currency to, DateTime date);
    public Task<CurrencyConversion> ConvertOnDateAsync(Currency from, List<Currency> currencies, DateTime date);
    public Task<IEnumerable<Currency>> GetSupportedCurrenciesAsync();
}