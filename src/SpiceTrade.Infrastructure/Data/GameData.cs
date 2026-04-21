using System.Text.Json.Serialization;

namespace SpiceTrade.Infrastructure.Data;

public sealed class GameData
{
    [JsonPropertyName("coins")]
    public List<CoinData> Coins { get; set; } = new();

    [JsonPropertyName("items")]
    public List<ItemData> Items { get; set; } = new();

    [JsonPropertyName("cities")]
    public List<CityData> Cities { get; set; } = new();

    [JsonPropertyName("contracts")]
    public List<ContractData> Contracts { get; set; } = new();
}

public sealed class CoinData
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("localizationKey")]
    public required string LocalizationKey { get; set; }

    [JsonPropertyName("mint")]
    public required string Mint { get; set; }

    [JsonPropertyName("denomination")]
    public decimal Denomination { get; set; }

    [JsonPropertyName("metalContent")]
    public decimal MetalContent { get; set; }
}

public sealed class ItemData
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("localizationKey")]
    public required string LocalizationKey { get; set; }

    [JsonPropertyName("basePrice")]
    public decimal BasePrice { get; set; }

    [JsonPropertyName("category")]
    public required string Category { get; set; }
}

public sealed class CityData
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("localizationKey")]
    public required string LocalizationKey { get; set; }

    [JsonPropertyName("region")]
    public required string Region { get; set; }

    [JsonPropertyName("priceModifiers")]
    public Dictionary<string, decimal> PriceModifiers { get; set; } = new();
}

public sealed class ContractData
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("localizationKey")]
    public required string LocalizationKey { get; set; }

    [JsonPropertyName("itemKey")]
    public required string ItemKey { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }

    [JsonPropertyName("originCityKey")]
    public required string OriginCityKey { get; set; }

    [JsonPropertyName("destinationCityKey")]
    public required string DestinationCityKey { get; set; }

    [JsonPropertyName("deadlineDays")]
    public int DeadlineDays { get; set; }

    [JsonPropertyName("paymentType")]
    public required string PaymentType { get; set; }

    [JsonPropertyName("paymentAmount")]
    public decimal PaymentAmount { get; set; }
}