using SpiceTrade.Core.Entities;

namespace SpiceTrade.Core.Interfaces;

public interface ICoinRepository
{
    Coin? Get(string coinKey);
    IReadOnlyList<Coin> GetAll();
}