namespace SpiceTrade.Core.Entities;

public sealed class Coin
{
    public required string LocalizationKey { get; init; }
    public required string Mint { get; init; }
    public required decimal Denomination { get; init; }
    public required decimal MetalContent { get; init; }

    public decimal GetValue() => Denomination * MetalContent;
}