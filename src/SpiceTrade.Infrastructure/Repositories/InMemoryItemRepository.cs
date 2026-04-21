using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Enums;
using SpiceTrade.Core.Interfaces;
using SpiceTrade.Infrastructure.Data;
using SpiceTrade.Infrastructure.Providers;

namespace SpiceTrade.Infrastructure.Repositories;

public sealed class InMemoryItemRepository : IItemRepository
{
    private readonly Dictionary<string, Item> _items;

    public InMemoryItemRepository(JsonDataProvider dataProvider)
    {
        var data = dataProvider.Load();
        _items = data.Items.ToDictionary(i => i.Key, i => new Item
        {
            LocalizationKey = i.LocalizationKey,
            BasePrice = i.BasePrice,
            Category = Enum.Parse<ItemCategory>(i.Category)
        });
    }

    public Item? Get(string itemKey) => _items.GetValueOrDefault(itemKey);

    public IReadOnlyList<Item> GetAll() => _items.Values.ToList();

    public IReadOnlyList<Item> GetByCategory(ItemCategory category) =>
        _items.Values.Where(i => i.Category == category).ToList();
}