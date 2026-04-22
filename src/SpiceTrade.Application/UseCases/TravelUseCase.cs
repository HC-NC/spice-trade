using SpiceTrade.Core.Interfaces;

namespace SpiceTrade.Application.UseCases;

public sealed class TravelInput
{
    public required string OriginCityKey { get; init; }
    public required string DestinationCityKey { get; init; }
}

public sealed class TravelUseCase : UseCase<TravelInput, UseCaseResult<TravelOutput>>
{
    private readonly ICityRepository _cityRepository;

    private static readonly Dictionary<string, Dictionary<string, int>> TravelTimes = new()
    {
        ["city_venice"] = new() { ["city_venice"] = 0, ["city_constantinople"] = 12, ["city_genoa"] = 4 },
        ["city_constantinople"] = new() { ["city_venice"] = 12, ["city_constantinople"] = 0, ["city_genoa"] = 10 },
        ["city_genoa"] = new() { ["city_venice"] = 4, ["city_constantinople"] = 10, ["city_genoa"] = 0 }
    };

    public TravelUseCase(ICityRepository cityRepository)
    {
        _cityRepository = cityRepository;
    }

    public override UseCaseResult<TravelOutput> Execute(TravelInput input)
    {
        if (input.OriginCityKey == input.DestinationCityKey)
            return new UseCaseResult<TravelOutput> { Success = false, Message = "Вы уже в этом городе" };

        var destination = _cityRepository.Get(input.DestinationCityKey);
        if (destination == null)
            return new UseCaseResult<TravelOutput> { Success = false, Message = "Город не найден" };

        var days = TravelTimes.GetValueOrDefault(input.OriginCityKey, new())
            .GetValueOrDefault(input.DestinationCityKey, 5);

        return new UseCaseResult<TravelOutput>
        {
            Success = true,
            Message = $"Переход в {destination.LocalizationKey}. Потрачено {days} дней.",
            Data = new TravelOutput
            {
                DestinationCityKey = input.DestinationCityKey,
                DaysTraveled = days
            }
        };
    }
}

public sealed class TravelOutput
{
    public required string DestinationCityKey { get; init; }
    public required int DaysTraveled { get; init; }
}