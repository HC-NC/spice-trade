using SpiceTrade.Core.Entities;
using SpiceTrade.Core.ValueObjects;

namespace SpiceTrade.Core;

public sealed class GameState
{
    public required string PlayerName { get; set; }
    public required string CurrentCityKey { get; set; }
    public required int Day { get; set; }
    public required int Month { get; set; }
    public required int Year { get; set; }
    
    public IReadOnlyDictionary<string, int> Inventory { get; set; } = new Dictionary<string, int>();
    public List<CoinState> Coins { get; set; } = new();
}

public sealed class CoinState
{
    public required string CoinTypeKey { get; set; }
    public required int Year { get; set; }
    public required int Condition { get; set; }
    public Dictionary<string, object> Properties { get; set; } = new();
}