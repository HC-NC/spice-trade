namespace SpiceTrade.Core.Entities;

public sealed class City
{
    public required string Key { get; init; }
    public required string LocalizationKey { get; init; }
    public required string Region { get; init; }
    public required IReadOnlyDictionary<string, decimal> PriceModifiers { get; init; }
}