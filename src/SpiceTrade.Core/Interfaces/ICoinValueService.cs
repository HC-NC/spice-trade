using SpiceTrade.Core.Entities;

namespace SpiceTrade.Core.Interfaces;

public interface ICoinValueService
{
    decimal GetNominalValue(Coin coin, string region);
    decimal GetMetalValue(Coin coin);
    decimal GetTotalValue(IEnumerable<Coin> coins, string region);
}