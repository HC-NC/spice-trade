using SpiceTrade.Core.Enums;

namespace SpiceTrade.Core.Entities;

public sealed class Item
{
    public required string LocalizationKey { get; init; }
    public required decimal BasePrice { get; init; }
    public required ItemCategory Category { get; init; }
}
