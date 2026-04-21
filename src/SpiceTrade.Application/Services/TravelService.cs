using SpiceTrade.Application.DTO;
using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Interfaces;
using SpiceTrade.Core.ValueObjects;

namespace SpiceTrade.Application.Services;

public sealed class TravelService
{
    private readonly ICityRepository _cityRepository;

    private static readonly Dictionary<string, Dictionary<string, int>> TravelTimes = new()
    {
        ["city_venice"] = new() { ["city_venice"] = 0, ["city_constantinople"] = 12, ["city_genoa"] = 4 },
        ["city_constantinople"] = new() { ["city_venice"] = 12, ["city_constantinople"] = 0, ["city_genoa"] = 10 },
        ["city_genoa"] = new() { ["city_venice"] = 4, ["city_constantinople"] = 10, ["city_genoa"] = 0 }
    };

    public TravelService(ICityRepository cityRepository)
    {
        _cityRepository = cityRepository;
    }

    public TravelResult Travel(Player player, string destinationCityKey, GameTime gameTime)
    {
        var originCityKey = player.CurrentCityKey;

        if (originCityKey == destinationCityKey)
            return new TravelResult { Success = false, Message = "Вы уже в этом городе" };

        var destination = _cityRepository.Get(destinationCityKey);
        if (destination == null)
            return new TravelResult { Success = false, Message = "Город не найден" };

        var days = TravelTimes.GetValueOrDefault(originCityKey, new())
            .GetValueOrDefault(destinationCityKey, 5);

        gameTime.Advance(days);
        player.CurrentCityKey = destinationCityKey;

        return new TravelResult
        {
            Success = true,
            Message = $"Переход в {destination.LocalizationKey}. Потрачено {days} дней.",
            DestinationCityKey = destinationCityKey,
            DaysTraveled = days
        };
    }
}