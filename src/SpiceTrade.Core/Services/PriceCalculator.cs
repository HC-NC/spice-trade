using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Enums;
using SpiceTrade.Core.Interfaces;

namespace SpiceTrade.Core.Services;

public sealed class PriceCalculator : IPriceCalculator
{
    private readonly IItemRepository _itemRepository;
    private readonly ICityRepository _cityRepository;

    private static readonly Dictionary<Season, Dictionary<string, decimal>> SeasonalModifiers = new()
    {
        [Season.Spring] = new() { ["Grain"] = 0.8m, ["Spice"] = 1.0m },
        [Season.Summer] = new() { ["Grain"] = 1.2m, ["Spice"] = 0.9m },
        [Season.Autumn] = new() { ["Grain"] = 1.0m, ["Spice"] = 1.1m },
        [Season.Winter] = new() { ["Grain"] = 1.5m, ["Spice"] = 1.3m }
    };

    public PriceCalculator(IItemRepository itemRepository, ICityRepository cityRepository)
    {
        _itemRepository = itemRepository;
        _cityRepository = cityRepository;
    }

    public decimal Calculate(string itemKey, string cityKey, Season season)
    {
        var item = _itemRepository.Get(itemKey);
        var city = _cityRepository.Get(cityKey);

        if (item == null || city == null)
            return 0;

        var basePrice = item.BasePrice;
        var regionModifier = city.PriceModifiers.GetValueOrDefault(itemKey, 1.0m);
        var categoryName = item.Category.ToString();
        var seasonModifier = SeasonalModifiers[season].GetValueOrDefault(categoryName, 1.0m);

        return basePrice * regionModifier * seasonModifier;
    }
}