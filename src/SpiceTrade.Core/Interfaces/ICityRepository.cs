using SpiceTrade.Core.Entities;

namespace SpiceTrade.Core.Interfaces;

public interface ICityRepository
{
    City? Get(string cityKey);
    IReadOnlyList<City> GetAll();
    IReadOnlyList<City> GetByRegion(string region);
}