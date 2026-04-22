using System.Collections.Generic;

namespace SpiceTrade.Application.Events;

public interface IGameEvent
{
    string EventType { get; }
    DateTime Timestamp { get; }
    Dictionary<string, object> Data { get; }
}

public interface IEventDispatcher
{
    void Subscribe(string eventType, Action<IGameEvent> handler);
    void Unsubscribe(string eventType, Action<IGameEvent> handler);
    void Publish(IGameEvent gameEvent);
}

public sealed class GameEventDispatcher : IEventDispatcher
{
    private readonly Dictionary<string, List<Action<IGameEvent>>> _handlers = new();

    public void Subscribe(string eventType, Action<IGameEvent> handler)
    {
        if (!_handlers.ContainsKey(eventType))
            _handlers[eventType] = new List<Action<IGameEvent>>();
        
        _handlers[eventType].Add(handler);
    }

    public void Unsubscribe(string eventType, Action<IGameEvent> handler)
    {
        if (_handlers.ContainsKey(eventType))
            _handlers[eventType].Remove(handler);
    }

    public void Publish(IGameEvent gameEvent)
    {
        if (_handlers.TryGetValue(gameEvent.EventType, out var handlers))
        {
            foreach (var handler in handlers.ToList())
            {
                handler(gameEvent);
            }
        }
    }
}

public abstract class GameEvent : IGameEvent
{
    public string EventType => GetType().Name;
    public DateTime Timestamp { get; } = DateTime.UtcNow;
    public Dictionary<string, object> Data { get; } = new();
}

public sealed class ItemBoughtEvent : GameEvent
{
    public required string ItemKey { get; init; }
    public required int Quantity { get; init; }
    public required decimal TotalPrice { get; init; }
}

public sealed class ItemSoldEvent : GameEvent
{
    public required string ItemKey { get; init; }
    public required int Quantity { get; init; }
    public required decimal TotalRevenue { get; init; }
}

public sealed class TravelEvent : GameEvent
{
    public required string FromCity { get; init; }
    public required string ToCity { get; init; }
    public required int DaysTraveled { get; init; }
}

public sealed class TimePassedEvent : GameEvent
{
    public required int Days { get; init; }
    public required int NewDay { get; init; }
    public required int NewMonth { get; init; }
    public required int NewYear { get; init; }
}