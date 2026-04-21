using System.Text.Json;
using SpiceTrade.Infrastructure.Data;

namespace SpiceTrade.Infrastructure.Providers;

public sealed class JsonDataProvider
{
    private readonly string _dataPath;
    private GameData? _cachedData;

    public JsonDataProvider(string dataPath = "Data")
    {
        _dataPath = dataPath;
    }

    public GameData Load()
    {
        if (_cachedData != null)
            return _cachedData;

        var filePath = Path.Combine(_dataPath, "gamedata.json");
        if (!File.Exists(filePath))
        {
            _cachedData = CreateDefaultData();
            return _cachedData;
        }

        var json = File.ReadAllText(filePath);
        _cachedData = JsonSerializer.Deserialize<GameData>(json) ?? CreateDefaultData();
        return _cachedData;
    }

    private static GameData CreateDefaultData()
    {
        return new GameData
        {
            Coins = new List<CoinData>
            {
                new() { Key = "coin_gold_ducat", LocalizationKey = "coin_gold_ducat", Mint = "Венеция", Denomination = 1, MetalContent = 1.0m },
                new() { Key = "coin_silver_groshen", LocalizationKey = "coin_silver_groshen", Mint = "Генуя", Denomination = 1, MetalContent = 0.5m }
            },
            Items = new List<ItemData>
            {
                new() { Key = "item_wheat", LocalizationKey = "item_wheat", BasePrice = 10, Category = "Grain" },
                new() { Key = "item_salt", LocalizationKey = "item_salt", BasePrice = 15, Category = "Grain" },
                new() { Key = "item_pepper", LocalizationKey = "item_pepper", BasePrice = 50, Category = "Spice" },
                new() { Key = "item_silk", LocalizationKey = "item_silk", BasePrice = 80, Category = "Cloth" },
                new() { Key = "item_iron", LocalizationKey = "item_iron", BasePrice = 25, Category = "Metal" }
            },
            Cities = new List<CityData>
            {
                new() { Key = "city_venice", LocalizationKey = "city_venice", Region = "Италия", PriceModifiers = new() { ["item_silk"] = 0.7m, ["item_salt"] = 1.2m } },
                new() { Key = "city_constantinople", LocalizationKey = "city_constantinople", Region = "Византия", PriceModifiers = new() { ["item_pepper"] = 0.6m, ["item_silk"] = 1.3m } },
                new() { Key = "city_genoa", LocalizationKey = "city_genoa", Region = "Италия", PriceModifiers = new() { ["item_iron"] = 0.8m, ["item_wheat"] = 1.1m } }
            },
            Contracts = new List<ContractData>
            {
                new() { Key = "contract_silk_venice_const", LocalizationKey = "contract_silk_venice_const", ItemKey = "item_silk", Quantity = 10, OriginCityKey = "city_venice", DestinationCityKey = "city_constantinople", DeadlineDays = 20, PaymentType = "ProfitShare", PaymentAmount = 0.3m }
            }
        };
    }
}