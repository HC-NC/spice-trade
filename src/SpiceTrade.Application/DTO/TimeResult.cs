namespace SpiceTrade.Application.DTO;

public sealed class TimeResult
{
    public required string Message { get; init; }
    public int DaysPassed { get; init; }
}