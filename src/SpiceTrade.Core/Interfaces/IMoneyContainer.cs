using SpiceTrade.Core.Entities;

namespace SpiceTrade.Core.Interfaces;

public interface IMoneyContainer
{
    string Name { get; }
    int Capacity { get; }
    IReadOnlyList<Coin> Coins { get; }
    int Count { get; }

    bool CanAdd(int quantity = 1);
    void Add(Coin coin);
    bool TryTake(Predicate<Coin> selector, out Coin? coin);
    IReadOnlyList<Coin> TakeAll();
    void Clear();
}

public abstract class MoneyContainerBase : IMoneyContainer
{
    public required string Name { get; init; }
    public required int Capacity { get; init; }
    protected readonly List<Coin> _coins = new();

    public IReadOnlyList<Coin> Coins => _coins.AsReadOnly();
    public int Count => _coins.Count;

    public bool CanAdd(int quantity = 1) => _coins.Count + quantity <= Capacity;

    public void Add(Coin coin)
    {
        if (!CanAdd()) return;
        _coins.Add(coin);
    }

    public bool TryTake(Predicate<Coin> selector, out Coin? coin)
    {
        var index = _coins.FindIndex(selector);
        if (index >= 0)
        {
            coin = _coins[index];
            _coins.RemoveAt(index);
            return true;
        }
        coin = null;
        return false;
    }

    public IReadOnlyList<Coin> TakeAll()
    {
        var all = _coins.ToList();
        _coins.Clear();
        return all;
    }

    public void Clear() => _coins.Clear();
}

public sealed class Wallet : MoneyContainerBase
{
    public Wallet() => Name = "Кошелёк";
}

public sealed class Purse : MoneyContainerBase
{
    public Purse() => Name = "Мешочек";
}

public sealed class Wagon : MoneyContainerBase
{
    public Wagon(int capacity = 100) => Capacity = capacity;
}