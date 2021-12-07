using NotionFinance.Models;

namespace NotionFinance.Services;

public interface ICryptocurrencyService
{
    public Task<IEnumerable<Cryptocurrency>> GetTopCryptocurrenciesAsync();
    public Task<Cryptocurrency> GetCryptocurrencyDetailsAsync(Cryptocurrency cryptocurrency);
}