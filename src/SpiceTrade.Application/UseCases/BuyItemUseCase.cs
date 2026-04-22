using SpiceTrade.Core.Enums;
using SpiceTrade.Core.Interfaces;

namespace SpiceTrade.Application.UseCases;

public sealed class BuyItemInput
{
    public required string PlayerId { get; init; }
    public required string ItemKey { get; init; }
    public required int Quantity { get; init; }
    public required string CityKey { get; init; }
    public required string Season { get; init; }
}

public sealed class BuyItemUseCase : UseCase<BuyItemInput, UseCaseResult<BuyItemOutput>>
{
    private readonly IItemRepository _itemRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IPriceCalculator _priceCalculator;

    public BuyItemUseCase(
        IItemRepository itemRepository,
        ICityRepository cityRepository,
        IPriceCalculator priceCalculator)
    {
        _itemRepository = itemRepository;
        _cityRepository = cityRepository;
        _priceCalculator = priceCalculator;
    }

    public override UseCaseResult<BuyItemOutput> Execute(BuyItemInput input)
    {
        if (input.Quantity <= 0)
            return new UseCaseResult<BuyItemOutput> { Success = false, Message = "Количество должно быть положительным" };

        var item = _itemRepository.Get(input.ItemKey);
        if (item == null)
            return new UseCaseResult<BuyItemOutput> { Success = false, Message = "Товар не найден" };

        var price = _priceCalculator.Calculate(input.ItemKey, input.CityKey, Enum.Parse<Season>(input.Season));
        var totalCost = price * input.Quantity;

        return new UseCaseResult<BuyItemOutput>
        {
            Success = true,
            Message = $"Куплено {input.Quantity} x {item.LocalizationKey} за {totalCost:F2}",
            Data = new BuyItemOutput
            {
                ItemKey = input.ItemKey,
                Quantity = input.Quantity,
                TotalPrice = totalCost,
                UnitPrice = price
            }
        };
    }
}

public sealed class BuyItemOutput
{
    public required string ItemKey { get; init; }
    public required int Quantity { get; init; }
    public required decimal TotalPrice { get; init; }
    public required decimal UnitPrice { get; init; }
}