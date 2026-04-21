using SpiceTrade.Core.Enums;

namespace SpiceTrade.Core.ValueObjects;

public sealed class GameTime
{
    public int Day { get; private set; } = 1;
    public int Month { get; private set; } = 1;
    public int Year { get; private set; } = 1;

    public Season Season => (Season)((Month - 1) / 3);

    public void Advance(int days)
    {
        Day += days;
        while (Day > 30)
        {
            Day -= 30;
            Month++;
            if (Month > 12)
            {
                Month = 1;
                Year++;
            }
        }
    }

    public override string ToString() => $"День {Day}, Месяц {Month}, Год {Year} ({Season})";
}