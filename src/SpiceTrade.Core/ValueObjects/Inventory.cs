using SpiceTrade.Core.Entities;

namespace SpiceTrade.Core.ValueObjects;

public sealed class Inventory
{
    private readonly Dictionary<string, int> _items = new();

    public int GetCount(string itemKey) => _items.GetValueOrDefault(itemKey, 0);

    public IReadOnlyDictionary<string, int> GetAll() => _items;

    public void Add(string itemKey, int quantity)
    {
        if (quantity <= 0) return;
        _items[itemKey] = GetCount(itemKey) + quantity;
    }

    public bool TryTake(string itemKey, int quantity)
    {
        if (quantity <= 0) return false;
        if (GetCount(itemKey) < quantity) return false;
        _items[itemKey] -= quantity;
        if (_items[itemKey] == 0)
            _items.Remove(itemKey);
        return true;
    }
}