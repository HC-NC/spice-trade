using SpiceTrade.Core.Entities;

namespace SpiceTrade.Core.Interfaces;

public interface IMoneyContainer
{
    string Name { get; }
    int Capacity { get; }
    int UsedSlots { get; }
    int TotalCoins { get; }
    IReadOnlyList<Coin> Coins { get; }
    IReadOnlyDictionary<string, int> Stacks { get; }

    bool CanAdd(int quantity = 1);
    void Add(Coin coin);
    void AddCoins(string coinTypeKey, int quantity, int year);
    bool TryTake(Predicate<Coin> selector, out Coin? coin);
    bool TryTakeCoins(string coinTypeKey, int quantity);
    IReadOnlyList<Coin> TakeAll();
    void Clear();
    void MergeStacks();
}

public abstract class MoneyContainerBase : IMoneyContainer
{
    public required string Name { get; init; }
    public required int Capacity { get; init; }
    protected readonly List<Coin> _coins = new();
    protected readonly Dictionary<string, CoinStack> _stacks = new();

    public IReadOnlyList<Coin> Coins => _coins.AsReadOnly();
    public IReadOnlyDictionary<string, int> Stacks => _stacks.ToDictionary(kv => kv.Key, kv => kv.Value.Quantity);
    public int UsedSlots => _stacks.Count;
    public int TotalCoins => _stacks.Values.Sum(s => s.Quantity);

    public bool CanAdd(int quantity = 1) => _stacks.Count + quantity <= Capacity;

    public void Add(Coin coin)
    {
        if (!CanAdd(1)) return;
        
        if (_stacks.TryGetValue(coin.CoinTypeKey, out var stack))
        {
            stack.Add(coin);
        }
        else
        {
            _stacks[coin.CoinTypeKey] = new CoinStack(coin.CoinTypeKey, coin.Year, coin.Condition);
            _stacks[coin.CoinTypeKey].Add(coin);
        }
    }

    public void AddCoins(string coinTypeKey, int quantity, int year)
    {
        if (!CanAdd(1)) return;
        
        if (_stacks.TryGetValue(coinTypeKey, out var stack))
        {
            for (int i = 0; i < quantity; i++)
            {
                stack.Add(new Coin { CoinTypeKey = coinTypeKey, Year = year, Condition = 100 });
            }
        }
        else
        {
            _stacks[coinTypeKey] = new CoinStack(coinTypeKey, year, 100);
            for (int i = 0; i < quantity; i++)
            {
                _stacks[coinTypeKey].Add(new Coin { CoinTypeKey = coinTypeKey, Year = year, Condition = 100 });
            }
        }
    }

    public bool TryTake(Predicate<Coin> selector, out Coin? coin)
    {
        foreach (var stack in _stacks.Values)
        {
            if (stack.TryTake(selector, out coin))
            {
                if (stack.Quantity == 0)
                    _stacks.Remove(stack.CoinTypeKey);
                return true;
            }
        }
        coin = null;
        return false;
    }

    public bool TryTakeCoins(string coinTypeKey, int quantity)
    {
        if (!_stacks.TryGetValue(coinTypeKey, out var stack))
            return false;

        if (stack.Quantity < quantity)
            return false;

        stack.Quantity -= quantity;
        if (stack.Quantity == 0)
            _stacks.Remove(coinTypeKey);

        return true;
    }

    public IReadOnlyList<Coin> TakeAll()
    {
        var all = _coins.ToList();
        _coins.Clear();
        _stacks.Clear();
        return all;
    }

    public void Clear()
    {
        _coins.Clear();
        _stacks.Clear();
    }

    public void MergeStacks()
    {
        // Already merged by design - stacks are automatic
    }
}

public sealed class CoinStack
{
    public string CoinTypeKey { get; }
    public int Year { get; }
    public int Condition { get; }
    public int Quantity { get; set; }
    private readonly List<Coin> _coins = new();

    public CoinStack(string coinTypeKey, int year, int condition)
    {
        CoinTypeKey = coinTypeKey;
        Year = year;
        Condition = condition;
        Quantity = 0;
    }

    public void Add(Coin coin)
    {
        _coins.Add(coin);
        Quantity++;
    }

    public bool TryTake(Predicate<Coin> selector, out Coin? coin)
    {
        var index = _coins.FindIndex(selector);
        if (index >= 0)
        {
            coin = _coins[index];
            _coins.RemoveAt(index);
            Quantity--;
            return true;
        }
        coin = null;
        return false;
    }
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