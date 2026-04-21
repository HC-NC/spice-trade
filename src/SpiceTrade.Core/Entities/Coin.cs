namespace SpiceTrade.Core.Entities;

public sealed class Coin
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string CoinTypeKey { get; init; }
    public required int Year { get; init; }
    public required int Condition { get; init; }
    public Dictionary<string, object> Properties { get; init; } = new();

    public decimal GetMetalValue(IMetalPriceProvider metalPrices)
    {
        var coinType = GetCoinType();
        if (coinType == null) return 0;

        decimal value = 0;
        foreach (var (metal, content) in coinType.MetalComposition)
        {
            var price = metalPrices.GetPrice(metal);
            value += content * price;
        }

        return value * (Condition / 100m);
    }

    private CoinType? GetCoinType() => null;
}

public sealed class CoinType
{
    public required string Key { get; init; }
    public required string LocalizationKey { get; init; }
    public required string Mint { get; init; }
    public required decimal BaseDenomination { get; init; }
    public required Dictionary<string, decimal> MetalComposition { get; init; }
}

public interface IMetalPriceProvider
{
    decimal GetPrice(string metal);
}