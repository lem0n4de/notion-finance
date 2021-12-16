using System.Text.Json;
using System.Text.Json.Nodes;
using Notion.Client;
using NotionFinance.Exceptions;
using NotionFinance.Models;
using Serilog;

namespace NotionFinance.Services.Forex;

public class ExchangeRateService : IForexApiService
{
    private HttpClient _httpClient;
    private static string ExchangeRatesBaseUrl = "https://api.exchangerate.host";

    public ExchangeRateService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CurrencyConversion> ConvertAsync(Currency from)
    {
        var result = await _httpClient.GetStringAsync($"{ExchangeRatesBaseUrl}/latest?base={from.Ticker}");
        var json = JsonNode.Parse(result);
        if (json == null) throw new ExchangeRateServiceError();

        var successNode = json["success"];
        if (successNode == null || successNode.ToString() == "false") throw new ExchangeRateServiceError();

        var date = json["date"]!.ToString();
        var d = DateTime.Parse(date);

        var rates = new List<Currency>();
        foreach (var (to, value) in json["rates"]!.AsObject())
        {
            try
            {
                var rate = new Currency() {Ticker = to, Value = float.Parse(value!.ToString())};
                rates.Add(rate);
            }
            catch (Exception e)
            {
                Log.Debug(e, "");
                throw new ExchangeRateServiceError();
            }
        }

        return new CurrencyConversion() {From = from, Date = d, Rates = rates};
    }

    public async Task<CurrencyConversion> ConvertAsync(Currency from, Currency to)
    {
        var result =
            await _httpClient.GetStringAsync($"{ExchangeRatesBaseUrl}/convert?from={from.Ticker}&to={to.Ticker}");
        var json = JsonNode.Parse(result);
        if (json == null) throw new ExchangeRateServiceError();

        var successNode = json["success"];
        if (successNode == null || successNode.ToString() == "false") throw new ExchangeRateServiceError();

        var date = json["date"]!.ToString();
        var d = DateTime.Parse(date);

        var resultNode = json["result"];
        if (resultNode == null) throw new ExchangeRateServiceError();

        var rate = float.Parse(resultNode.ToString());
        return new CurrencyConversion()
        {
            From = from,
            Date = d,
            Rates = new List<Currency>() {new Currency() {Name = to.Name, Ticker = to.Ticker, Value = rate}}
        };
    }

    public async Task<CurrencyConversion> ConvertAsync(Currency from, List<Currency> currencies)
    {
        var symbolsString = string.Join(",", currencies.Select(x => x.Ticker));
        var result =
            await _httpClient.GetStringAsync(
                $"{ExchangeRatesBaseUrl}/latest?base={from.Ticker}&symbols={symbolsString}");

        var json = JsonNode.Parse(result);
        if (json == null) throw new ExchangeRateServiceError();

        var successNode = json["success"];
        if (successNode == null || successNode.ToString() == "false") throw new ExchangeRateServiceError();

        var date = json["date"]!.ToString();
        var d = DateTime.Parse(date);

        var rates = new List<Currency>();
        foreach (var (to, value) in json["rates"]!.AsObject())
        {
            try
            {
                var rate = new Currency() {Ticker = to, Value = float.Parse(value!.ToString())};
                rates.Add(rate);
            }
            catch (Exception e)
            {
                Log.Debug(e, "");
                throw new ExchangeRateServiceError();
            }
        }

        return new CurrencyConversion() {From = from, Date = d, Rates = rates};
    }

    public async Task<CurrencyConversion> ConvertOnDateAsync(Currency from, DateTime dateTime)
    {
        var result =
            await _httpClient.GetStringAsync(
                $"{ExchangeRatesBaseUrl}/{dateTime.Year}-{dateTime.Month}-{dateTime.Day}?base={from.Ticker}");
        var json = JsonNode.Parse(result);
        if (json == null) throw new ExchangeRateServiceError();

        var successNode = json["success"];
        if (successNode == null || successNode.ToString() == "false") throw new ExchangeRateServiceError();

        var date = json["date"]!.ToString();
        var d = DateTime.Parse(date);

        var rates = new List<Currency>();
        foreach (var (to, value) in json["rates"]!.AsObject())
        {
            try
            {
                var rate = new Currency() {Ticker = to, Value = float.Parse(value!.ToString())};
                rates.Add(rate);
            }
            catch (Exception e)
            {
                Log.Debug(e, "");
                throw new ExchangeRateServiceError();
            }
        }

        return new CurrencyConversion() {From = from, Date = d, Rates = rates};
    }

    public async Task<CurrencyConversion> ConvertOnDateAsync(Currency from, Currency to, DateTime dateTime)
    {
        var result =
            await _httpClient.GetStringAsync(
                $"{ExchangeRatesBaseUrl}/convert?base={from.Ticker}&to={to.Ticker}&date={dateTime.Year}-{dateTime.Month}-{dateTime.Day}");
        var json = JsonNode.Parse(result);
        if (json == null) throw new ExchangeRateServiceError();

        var successNode = json["success"];
        if (successNode == null || successNode.ToString() == "false") throw new ExchangeRateServiceError();

        var date = json["date"]!.ToString();
        var d = DateTime.Parse(date);

        var resultNode = json["result"];
        if (resultNode == null) throw new ExchangeRateServiceError();

        var rate = float.Parse(resultNode.ToString());
        return new CurrencyConversion()
        {
            From = from,
            Date = d,
            Rates = new List<Currency>() {new Currency() {Name = to.Name, Ticker = to.Ticker, Value = rate}}
        };
    }

    public async Task<CurrencyConversion> ConvertOnDateAsync(Currency from, List<Currency> currencies,
        DateTime dateTime)
    {
        var symbolsString = string.Join(",", currencies.Select(x => x.Ticker));
        var result =
            await _httpClient.GetStringAsync(
                $"{ExchangeRatesBaseUrl}/{dateTime.Year}-{dateTime.Month}-{dateTime.Day}?base={from.Ticker}&symbols={symbolsString}");

        var json = JsonNode.Parse(result);
        if (json == null) throw new ExchangeRateServiceError();

        var successNode = json["success"];
        if (successNode == null || successNode.ToString() == "false") throw new ExchangeRateServiceError();

        var date = json["date"]!.ToString();
        var d = DateTime.Parse(date);

        var rates = new List<Currency>();
        foreach (var (to, value) in json["rates"]!.AsObject())
        {
            try
            {
                var rate = new Currency() {Ticker = to, Value = float.Parse(value!.ToString())};
                rates.Add(rate);
            }
            catch (Exception e)
            {
                Log.Debug(e, "");
                throw new ExchangeRateServiceError();
            }
        }

        return new CurrencyConversion() {From = from, Date = d, Rates = rates};
    }

    public async Task<IEnumerable<Currency>> GetSupportedCurrenciesAsync()
    {
        var result = await _httpClient.GetStringAsync($"{ExchangeRatesBaseUrl}/symbols");
        var json = JsonNode.Parse(result);
        if (json == null) throw new ExchangeRateServiceError();

        var successNode = json["success"];
        if (successNode == null || successNode.ToString() == "false") throw new ExchangeRateServiceError();

        var symbolsNode = json["symbols"];
        if (symbolsNode == null) throw new ExchangeRateServiceError();

        var currencies = new List<Currency>();
        foreach (var (name, value) in symbolsNode.AsObject())
        {
            currencies.Add(new Currency() {Name = name, Ticker = value!["description"]!.ToString(), Value = 1});
        }

        return currencies;
    }
}