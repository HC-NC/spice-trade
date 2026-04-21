using System.Collections.Generic;
using SpiceTrade.Core.Entities;

namespace SpiceTrade.Core.ValueObjects;

public sealed class Wallet
{
    private readonly Dictionary<string, int> _coins = new();

    public int GetCount(string coinKey) => _coins.GetValueOrDefault(coinKey, 0);

    public IReadOnlyDictionary<string, int> GetAll() => _coins;

    public void Add(string coinKey, int quantity)
    {
        if (quantity <= 0) return;
        _coins[coinKey] = GetCount(coinKey) + quantity;
    }

    public bool TryTake(string coinKey, int quantity)
    {
        if (quantity <= 0) return false;
        if (GetCount(coinKey) < quantity) return false;
        _coins[coinKey] -= quantity;
        if (_coins[coinKey] == 0)
            _coins.Remove(coinKey);
        return true;
    }

    public decimal GetTotalValue(Func<string, Coin?> coinResolver)
    {
        decimal total = 0;
        foreach (var (coinKey, count) in _coins)
        {
            var coin = coinResolver(coinKey);
            if (coin != null)
                total += coin.GetValue() * count;
        }
        return total;
    }

    public bool TryTakeByValue(decimal amount, Func<string, Coin?> coinResolver)
    {
        if (amount <= 0) return false;
        var available = GetTotalValue(coinResolver);
        if (available < amount) return false;

        var remaining = amount;
        var coinsToTake = new Dictionary<string, int>();

        foreach (var (coinKey, count) in _coins.ToList())
        {
            var coin = coinResolver(coinKey);
            if (coin == null) continue;

            var coinValue = coin.GetValue();
            var maxAffordable = (int)(remaining / coinValue);
            var toTake = Math.Min(count, maxAffordable);

            if (toTake > 0)
            {
                coinsToTake[coinKey] = toTake;
                remaining -= coinValue * toTake;
            }

            if (remaining <= 0.01m) break;
        }

        foreach (var (coinKey, count) in coinsToTake)
        {
            TryTake(coinKey, count);
        }

        return true;
    }
}