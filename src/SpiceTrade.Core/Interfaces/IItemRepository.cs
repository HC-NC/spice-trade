using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Enums;

namespace SpiceTrade.Core.Interfaces;

public interface IItemRepository
{
    Item? Get(string itemKey);
    IReadOnlyList<Item> GetAll();
    IReadOnlyList<Item> GetByCategory(ItemCategory category);
}