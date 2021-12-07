using System.Text.Json.Serialization;

namespace NotionFinance.Models;

public class Cryptocurrency
{
    [JsonPropertyName("id")] public string CoinGeckoId { get; set; }
    [JsonPropertyName("symbol")] public string? Symbol { get; set; }
    [JsonPropertyName("name")] public string? Name { get; set; }
    [JsonPropertyName("image")] public string? ImageUrl { get; set; }
    [JsonPropertyName("current_price")] public float? CurrentPrice { get; set; }
    [JsonPropertyName("market_cap")] public float? MarketCap { get; set; }
    [JsonPropertyName("market_cap_rank")] public int? MarketCapRank { get; set; }

    [JsonPropertyName("fully_diluted_valuation")]
    public float? FullyDilutedValuation { get; set; }

    [JsonPropertyName("total_volume")] public float? TotalVolume { get; set; }
    [JsonPropertyName("high_24h")] public float? High24H { get; set; }
    [JsonPropertyName("low_24h")] public float? Low24H { get; set; }
    [JsonPropertyName("price_change_24h")] public float? PriceChange24H { get; set; }

    [JsonPropertyName("price_change_percentage_24h")]
    public float? PriceChangePercentage24H { get; set; }

    [JsonPropertyName("market_cap_change_24h")]
    public float? MarketCapChange24H { get; set; }

    [JsonPropertyName("market_cap_change_percentage_24h")]
    public float? MarketCapChangePercentage24H { get; set; }

    [JsonPropertyName("circulating_supply")]
    public float? CirculatingSupply { get; set; }

    [JsonPropertyName("total_supply")] public float? TotalSupply { get; set; }
    [JsonPropertyName("max_supply")] public float? MaxSupply { get; set; }
    [JsonPropertyName("ath")] public float? ATH { get; set; }

    [JsonPropertyName("ath_change_percentage")]
    public float? AthChangePercentage { get; set; }

    [JsonPropertyName("ath_date")] public DateTime? AthDate { get; set; }
    [JsonPropertyName("atl")] public float? ATL { get; set; }

    [JsonPropertyName("atl_change_percentage")]
    public float? AtlChangePercentage { get; set; }

    [JsonPropertyName("atl_date")] public DateTime? AtlDate { get; set; }
    [JsonPropertyName("roi")] public object? Roi { get; set; }
    [JsonPropertyName("last_updated")] public DateTime? LastUpdated { get; set; }
}