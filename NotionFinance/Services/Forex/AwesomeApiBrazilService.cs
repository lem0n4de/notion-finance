using System.Text.Json.Nodes;
using AlphaVantage.Net.Common.Currencies;
using NotionFinance.Exceptions;
using NotionFinance.Models;
using Serilog;

namespace NotionFinance.Services.Forex;

public class AwesomeApiBrazilService : IForexService
{
    private HttpClient _httpClient;

    public AwesomeApiBrazilService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static string GetUrlForConversion(string fromTicker, string toTicker) =>
        $"https://economia.awesomeapi.com.br/json/last/{fromTicker.ToUpper()}-{toTicker.ToUpper()}";

    public async Task<CurrencyConversion> ConvertAsync(Currency @from, Currency to)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, GetUrlForConversion(from.Ticker, to.Ticker));
            var res = await _httpClient.SendAsync(req);
            res.EnsureSuccessStatusCode();
            var json = JsonNode.Parse(await res.Content.ReadAsStringAsync());
            var symbol = json![$"{from.Ticker.ToUpper()}{to.Ticker.ToUpper()}"];
            var currentPrice = symbol!["bid"]!.ToString();
            var d = symbol["create_date"]!.ToString();
            var date = DateTime.Parse(d);

            return new CurrencyConversion()
            {
                Date = date,
                From = from,
                Rates = new List<Currency>()
                {
                    new Currency() {Name = to.Name, Ticker = to.Ticker, Value = float.Parse(currentPrice)}
                }
            };
        }
        catch (NullReferenceException e)
        {
            Log.Debug(e, "Failed attempt of currency conversion from AwesomeApiBrazilService");
            throw new ForexServiceError();
        }
        catch (HttpRequestException e)
        {
            Log.Debug(e, "");
            throw new ForexServiceError();
        }
    }

    public async Task<CurrencyConversion> ConvertAsync(Currency @from, List<Currency> currencies)
    {
        var rates = new List<Currency>();
        var tasks = new List<Task<CurrencyConversion>>();
        foreach (var currency in currencies)
        {
            try
            {
                var conversion = await ConvertAsync(from, currency);
                rates.Add(conversion.Rates[0]);
            }
            catch (ForexServiceError e)
            {
                Log.Debug(e, "Error while converting {Ticker}", currency.Ticker);
                continue;
            }
        }

        return new CurrencyConversion()
        {
            Date = DateTime.Now,
            From = from,
            Rates = rates
        };
    }

    public Task<CurrencyConversion> ConvertOnDateAsync(Currency @from, Currency to, DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<CurrencyConversion> ConvertOnDateAsync(Currency @from, List<Currency> currencies, DateTime date)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Currency>> GetSupportedCurrenciesAsync()
    {
        throw new NotImplementedException();
    }
}