using SpiceTrade.Core.ValueObjects;

namespace SpiceTrade.Core.Entities;

public sealed class Player
{
    public required string Name { get; init; }
    public required Wallet Wallet { get; init; }
    public required Inventory Inventory { get; init; }
    public required string CurrentCityKey { get; set; }
}