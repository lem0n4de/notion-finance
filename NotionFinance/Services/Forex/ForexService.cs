using NotionFinance.Exceptions;
using NotionFinance.Models;
using Serilog;

namespace NotionFinance.Services.Forex;

public class ForexService : IForexService
{
    private readonly IDictionary<string, IForexApiService> _forexApiServices;
    private const string AWESOMEAPIDICTKEY = "AWESOME";
    private const string ALPHAVANTAGEDICTKEY = "ALPHAVANTAGE";
    private const string EXCHANGERATEDICTKEY = "EXCHANGERATE";

    public ForexService(IEnumerable<IForexApiService> forexApiServices)
    {
        var forexApis = forexApiServices.ToList();

        _forexApiServices = new Dictionary<string, IForexApiService>();
        var awesome = forexApis.Find(x => x is AwesomeApiBrazilService);
        if (awesome != null) _forexApiServices.Add(AWESOMEAPIDICTKEY, awesome);

        var alphaVantage = forexApis.Find(x => x is AlphaVantageService);
        if (alphaVantage != null) _forexApiServices.Add(ALPHAVANTAGEDICTKEY, alphaVantage);

        var exchangeRate = forexApis.Find(x => x is ExchangeRateService);
        if (exchangeRate != null) _forexApiServices.Add(EXCHANGERATEDICTKEY, exchangeRate);
    }

    public async Task<CurrencyConversion> ConvertAsync(Currency @from, Currency to)
    {
        CurrencyConversion? returnVal = null;
        try
        {
            returnVal = await _forexApiServices[AWESOMEAPIDICTKEY].ConvertAsync(from, to);
        }
        catch (KeyNotFoundException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", AWESOMEAPIDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", AWESOMEAPIDICTKEY);
        }

        if (returnVal != null) return returnVal;
        try
        {
            returnVal = await _forexApiServices[ALPHAVANTAGEDICTKEY].ConvertAsync(@from, to);
        }
        catch (KeyNotFoundException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", ALPHAVANTAGEDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", ALPHAVANTAGEDICTKEY);
        }

        if (returnVal != null) return returnVal;
        try
        {
            returnVal = await _forexApiServices[EXCHANGERATEDICTKEY].ConvertAsync(from, to);
        }
        catch (KeyNotFoundException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", EXCHANGERATEDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", EXCHANGERATEDICTKEY);
        }

        if (returnVal == null) throw new ForexServiceException();
        return returnVal;
    }

    public async Task<CurrencyConversion> ConvertAsync(Currency @from, List<Currency> currencies)
    {
        CurrencyConversion? returnVal = null;
        try
        {
            returnVal = await _forexApiServices[AWESOMEAPIDICTKEY].ConvertAsync(from, currencies);
        }
        catch (ArgumentNullException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", AWESOMEAPIDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", AWESOMEAPIDICTKEY);
        }

        if (returnVal != null) return returnVal;
        try
        {
            returnVal = await _forexApiServices[ALPHAVANTAGEDICTKEY].ConvertAsync(@from, currencies);
        }
        catch (ArgumentNullException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", ALPHAVANTAGEDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", ALPHAVANTAGEDICTKEY);
        }

        if (returnVal != null) return returnVal;
        try
        {
            returnVal = await _forexApiServices[EXCHANGERATEDICTKEY].ConvertAsync(from, currencies);
        }
        catch (ArgumentNullException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", EXCHANGERATEDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", EXCHANGERATEDICTKEY);
        }

        if (returnVal == null) throw new ForexServiceException();
        return returnVal;
    }

    public async Task<CurrencyConversion> ConvertOnDateAsync(Currency @from, Currency to, DateTime date)
    {
        CurrencyConversion? returnVal = null;
        try
        {
            returnVal = await _forexApiServices[AWESOMEAPIDICTKEY].ConvertOnDateAsync(from, to, date);
        }
        catch (ArgumentNullException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", AWESOMEAPIDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", AWESOMEAPIDICTKEY);
        }

        if (returnVal != null) return returnVal;
        try
        {
            returnVal = await _forexApiServices[ALPHAVANTAGEDICTKEY].ConvertOnDateAsync(@from, to, date);
        }
        catch (ArgumentNullException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", ALPHAVANTAGEDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", ALPHAVANTAGEDICTKEY);
        }

        if (returnVal != null) return returnVal;
        try
        {
            returnVal = await _forexApiServices[EXCHANGERATEDICTKEY].ConvertOnDateAsync(from, to, date);
        }
        catch (ArgumentNullException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", EXCHANGERATEDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", EXCHANGERATEDICTKEY);
        }

        if (returnVal == null) throw new ForexServiceException();
        return returnVal;
    }

    public async Task<CurrencyConversion> ConvertOnDateAsync(Currency @from, List<Currency> currencies, DateTime date)
    {
        CurrencyConversion? returnVal = null;
        try
        {
            returnVal = await _forexApiServices[AWESOMEAPIDICTKEY].ConvertOnDateAsync(from, currencies, date);
        }
        catch (ArgumentNullException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", AWESOMEAPIDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", AWESOMEAPIDICTKEY);
        }

        if (returnVal != null) return returnVal;
        try
        {
            returnVal = await _forexApiServices[ALPHAVANTAGEDICTKEY].ConvertOnDateAsync(@from, currencies, date);
        }
        catch (ArgumentNullException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", ALPHAVANTAGEDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", ALPHAVANTAGEDICTKEY);
        }

        if (returnVal != null) return returnVal;
        try
        {
            returnVal = await _forexApiServices[EXCHANGERATEDICTKEY].ConvertOnDateAsync(from, currencies, date);
        }
        catch (ArgumentNullException e)
        {
            Log.Debug(e, "{Key} not found in Api Services", EXCHANGERATEDICTKEY);
        }
        catch (ForexServiceException e)
        {
            Log.Debug(e, "Error in {Key}", EXCHANGERATEDICTKEY);
        }

        if (returnVal == null) throw new ForexServiceException();
        return returnVal;
    }

    public Task<IEnumerable<Currency>> GetSupportedCurrenciesAsync()
    {
        return Task.FromResult<IEnumerable<Currency>>(new List<Currency>());
    }
}