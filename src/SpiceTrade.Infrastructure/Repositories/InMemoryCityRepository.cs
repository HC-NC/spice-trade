using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Interfaces;
using SpiceTrade.Infrastructure.Data;
using SpiceTrade.Infrastructure.Providers;

namespace SpiceTrade.Infrastructure.Repositories;

public sealed class InMemoryCityRepository : ICityRepository
{
    private readonly Dictionary<string, City> _cities;

    public InMemoryCityRepository(JsonDataProvider dataProvider)
    {
        var data = dataProvider.Load();
        _cities = data.Cities.ToDictionary(c => c.Key, c => new City
        {
            Key = c.Key,
            LocalizationKey = c.LocalizationKey,
            Region = c.Region,
            PriceModifiers = c.PriceModifiers
        });
    }

    public City? Get(string cityKey) => _cities.GetValueOrDefault(cityKey);

    public IReadOnlyList<City> GetAll() => _cities.Values.ToList();

    public IReadOnlyList<City> GetByRegion(string region) =>
        _cities.Values.Where(c => c.Region == region).ToList();
}