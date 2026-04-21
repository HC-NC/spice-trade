using SpiceTrade.Application.DTO;
using SpiceTrade.Core.ValueObjects;

namespace SpiceTrade.Application.Services;

public sealed class TimeService
{
    public TimeResult Wait(GameTime gameTime, int days)
    {
        if (days <= 0)
            return new TimeResult { Message = "Количество дней должно быть положительным", DaysPassed = 0 };

        gameTime.Advance(days);

        return new TimeResult
        {
            Message = $"Прошло {days} дней. {gameTime}",
            DaysPassed = days
        };
    }
}