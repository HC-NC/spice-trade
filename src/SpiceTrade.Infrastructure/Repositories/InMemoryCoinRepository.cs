using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Interfaces;
using SpiceTrade.Infrastructure.Providers;

namespace SpiceTrade.Infrastructure.Repositories;

public sealed class InMemoryCoinTypeRepository : ICoinTypeRepository
{
    private readonly Dictionary<string, CoinType> _coinTypes;

    public InMemoryCoinTypeRepository(JsonDataProvider dataProvider)
    {
        var data = dataProvider.Load();
        _coinTypes = data.Coins.ToDictionary(c => c.Key, c => new CoinType
        {
            Key = c.Key,
            LocalizationKey = c.LocalizationKey,
            Mint = c.Mint,
            BaseDenomination = c.Denomination,
            MetalComposition = c.MetalComposition.Count > 0 ? c.MetalComposition : new Dictionary<string, decimal> { ["Gold"] = c.Denomination }
        });
    }

    public CoinType? Get(string coinTypeKey) => _coinTypes.GetValueOrDefault(coinTypeKey);

    public IReadOnlyList<CoinType> GetAll() => _coinTypes.Values.ToList();
}

public sealed class InMemoryCoinRepository : ICoinRepository
{
    private readonly List<Coin> _coins = new();

    public Coin Create(string coinTypeKey, int year)
    {
        var coin = new Coin
        {
            CoinTypeKey = coinTypeKey,
            Year = year,
            Condition = 100
        };
        _coins.Add(coin);
        return coin;
    }

    public IReadOnlyList<Coin> GetAll() => _coins.AsReadOnly();

    public void Add(Coin coin) => _coins.Add(coin);

    public bool Remove(Guid coinId)
    {
        var coin = _coins.FirstOrDefault(c => c.Id == coinId);
        if (coin != null)
        {
            _coins.Remove(coin);
            return true;
        }
        return false;
    }
}