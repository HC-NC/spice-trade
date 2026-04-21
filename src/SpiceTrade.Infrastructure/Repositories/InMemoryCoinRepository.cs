using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Interfaces;
using SpiceTrade.Infrastructure.Data;
using SpiceTrade.Infrastructure.Providers;

namespace SpiceTrade.Infrastructure.Repositories;

public sealed class InMemoryCoinRepository : ICoinRepository
{
    private readonly Dictionary<string, Coin> _coins;

    public InMemoryCoinRepository(JsonDataProvider dataProvider)
    {
        var data = dataProvider.Load();
        _coins = data.Coins.ToDictionary(c => c.Key, c => new Coin
        {
            LocalizationKey = c.LocalizationKey,
            Mint = c.Mint,
            Denomination = c.Denomination,
            MetalContent = c.MetalContent
        });
    }

    public Coin? Get(string coinKey) => _coins.GetValueOrDefault(coinKey);

    public IReadOnlyList<Coin> GetAll() => _coins.Values.ToList();
}