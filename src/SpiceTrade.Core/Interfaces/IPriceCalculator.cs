using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Enums;

namespace SpiceTrade.Core.Interfaces;

public interface IPriceCalculator
{
    decimal Calculate(string itemKey, string cityKey, Season season);
}