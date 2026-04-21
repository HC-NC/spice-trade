using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Interfaces;

namespace SpiceTrade.Core.Services;

public sealed class SimpleMetalPriceProvider : IMetalPriceProvider
{
    private static readonly Dictionary<string, decimal> Prices = new()
    {
        ["Gold"] = 100,
        ["Silver"] = 10,
        ["Copper"] = 1
    };

    public decimal GetPrice(string metal) => Prices.GetValueOrDefault(metal, 0);
}

public sealed class SimpleCoinValueService : Interfaces.ICoinValueService
{
    private readonly Interfaces.ICoinTypeRepository _coinTypeRepository;
    private readonly IMetalPriceProvider _metalPrices;

    public SimpleCoinValueService(Interfaces.ICoinTypeRepository coinTypeRepository, IMetalPriceProvider metalPrices)
    {
        _coinTypeRepository = coinTypeRepository;
        _metalPrices = metalPrices;
    }

    public decimal GetNominalValue(Entities.Coin coin, string region)
    {
        var coinType = _coinTypeRepository.Get(coin.CoinTypeKey);
        if (coinType == null) return 0;
        return coinType.BaseDenomination;
    }

    public decimal GetMetalValue(Entities.Coin coin)
    {
        var coinType = _coinTypeRepository.Get(coin.CoinTypeKey);
        if (coinType == null) return 0;

        decimal value = 0;
        foreach (var (metal, content) in coinType.MetalComposition)
        {
            value += content * _metalPrices.GetPrice(metal);
        }

        return value * (coin.Condition / 100m);
    }

    public decimal GetTotalValue(IEnumerable<Entities.Coin> coins, string region)
    {
        decimal total = 0;
        foreach (var coin in coins)
        {
            var nominal = GetNominalValue(coin, region);
            var metal = GetMetalValue(coin);
            total += Math.Max(nominal, metal);
        }
        return total;
    }
}