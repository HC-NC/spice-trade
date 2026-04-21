using SpiceTrade.Core.Entities;

namespace SpiceTrade.Core.Interfaces;

public interface ICoinTypeRepository
{
    CoinType? Get(string coinTypeKey);
    IReadOnlyList<CoinType> GetAll();
}

public interface ICoinRepository
{
    Coin Create(string coinTypeKey, int year);
    IReadOnlyList<Coin> GetAll();
    void Add(Coin coin);
    bool Remove(Guid coinId);
}