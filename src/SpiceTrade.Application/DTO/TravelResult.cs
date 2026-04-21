namespace SpiceTrade.Application.DTO;

public sealed class TravelResult
{
    public required bool Success { get; init; }
    public required string Message { get; init; }
    public string? DestinationCityKey { get; init; }
    public int DaysTraveled { get; init; }
}