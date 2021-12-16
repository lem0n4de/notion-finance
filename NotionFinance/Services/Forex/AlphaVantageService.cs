using AlphaVantage.Net.Common.Currencies;
using AlphaVantage.Net.Core.Client;
using AlphaVantage.Net.Forex;
using AlphaVantage.Net.Forex.Client;
using NotionFinance.Exceptions;
using NotionFinance.Models;
using static System.Enum;

namespace NotionFinance.Services.Forex;

public class AlphaVantageService : IForexApiService
{
    private string _alphaVantageApiKey;

    public AlphaVantageService(string apiKey)
    {
        _alphaVantageApiKey = apiKey;
    }

    public Task<CurrencyConversion> ConvertAsync(Currency @from)
    {
        throw new NotImplementedException();
    }

    public async Task<CurrencyConversion> ConvertAsync(Currency @from, Currency to)
    {
        using var client = new AlphaVantageClient(_alphaVantageApiKey);
        using var forexClient = client.Forex();

        PhysicalCurrency f;
        PhysicalCurrency t;
        var success1 = TryParse(@from.Ticker, out f);
        var success2 = TryParse(to.Ticker, out t);
        if (!success1 || !success2)
        {
            throw new ForexServiceException();
        }

        var forexExchangeRate =
            await forexClient.GetExchangeRateAsync(f, t);
        var conversion = new CurrencyConversion()
        {
            From = new Currency()
            {
                Name = forexExchangeRate.FromCurrencyName, Ticker = forexExchangeRate.FromCurrencyCode.ToString(),
                Value = 1
            },
            Date = forexExchangeRate.LastRefreshed,
            Rates = new List<Currency>()
            {
                new Currency()
                {
                    Name = forexExchangeRate.ToCurrencyName, Ticker = forexExchangeRate.ToCurrencyCode.ToString(),
                    Value = (float?) forexExchangeRate.AskPrice
                }
            }
        };
        return conversion;
    }

    public Task<CurrencyConversion> ConvertAsync(Currency @from, List<Currency> currencies)
    {
        throw new NotImplementedException();
    }

    public Task<CurrencyConversion> ConvertOnDateAsync(Currency @from, DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<CurrencyConversion> ConvertOnDateAsync(Currency @from, Currency to, DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<CurrencyConversion> ConvertOnDateAsync(Currency @from, List<Currency> currencies, DateTime date)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Currency>> GetSupportedCurrenciesAsync()
    {
        return new List<Currency>();
    }
}