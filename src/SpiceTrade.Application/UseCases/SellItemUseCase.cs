using SpiceTrade.Core.Enums;
using SpiceTrade.Core.Interfaces;

namespace SpiceTrade.Application.UseCases;

public sealed class SellItemInput
{
    public required string ItemKey { get; init; }
    public required int Quantity { get; init; }
    public required string CityKey { get; init; }
    public required string Season { get; init; }
}

public sealed class SellItemUseCase : UseCase<SellItemInput, UseCaseResult<SellItemOutput>>
{
    private readonly IItemRepository _itemRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IPriceCalculator _priceCalculator;

    public SellItemUseCase(
        IItemRepository itemRepository,
        ICityRepository cityRepository,
        IPriceCalculator priceCalculator)
    {
        _itemRepository = itemRepository;
        _cityRepository = cityRepository;
        _priceCalculator = priceCalculator;
    }

    public override UseCaseResult<SellItemOutput> Execute(SellItemInput input)
    {
        if (input.Quantity <= 0)
            return new UseCaseResult<SellItemOutput> { Success = false, Message = "Количество должно быть положительным" };

        var item = _itemRepository.Get(input.ItemKey);
        if (item == null)
            return new UseCaseResult<SellItemOutput> { Success = false, Message = "Товар не найден" };

        var price = _priceCalculator.Calculate(input.ItemKey, input.CityKey, Enum.Parse<Season>(input.Season));
        var totalRevenue = price * input.Quantity;

        return new UseCaseResult<SellItemOutput>
        {
            Success = true,
            Message = $"Продано {input.Quantity} x {item.LocalizationKey} за {totalRevenue:F2}",
            Data = new SellItemOutput
            {
                ItemKey = input.ItemKey,
                Quantity = input.Quantity,
                TotalRevenue = totalRevenue,
                UnitPrice = price
            }
        };
    }
}

public sealed class SellItemOutput
{
    public required string ItemKey { get; init; }
    public required int Quantity { get; init; }
    public required decimal TotalRevenue { get; init; }
    public required decimal UnitPrice { get; init; }
}