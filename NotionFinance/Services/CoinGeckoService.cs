using System.Text.Json;
using NotionFinance.Exceptions;
using NotionFinance.Models;

namespace NotionFinance.Services;

public class CoinGeckoService: ICryptocurrencyService
{
    private HttpClient _httpClient;

    public CoinGeckoService()
    {
        _httpClient = new HttpClient();
    }
    public async Task<IEnumerable<Cryptocurrency>> GetTopCryptocurrenciesAsync()
    {
        var result = await _httpClient.GetStringAsync(
            "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc");
        var r = JsonSerializer.Deserialize<IEnumerable<Cryptocurrency>>(result);
        if (r == null) throw new CoinGeckoServiceError();
        return r;
    }

    public async Task<Cryptocurrency> GetCryptocurrencyDetailsAsync(Cryptocurrency cryptocurrency)
    {
        var result =
            await _httpClient.GetStringAsync(
                $"https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&ids={cryptocurrency.CoinGeckoId}");
        var r = JsonSerializer.Deserialize<List<Cryptocurrency>>(result);
        if (r == null || !r.Any())
        {
            throw new CoinGeckoServiceError();
        }

        return r.First();
    }
}
