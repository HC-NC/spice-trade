namespace SpiceTrade.Application.DTO;

public sealed class TradeResult
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
    public decimal TotalPrice { get; init; }
    public int QuantityTraded { get; init; }
}