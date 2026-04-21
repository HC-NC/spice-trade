using SpiceTrade.Application.DTO;
using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Enums;
using SpiceTrade.Core.Interfaces;
using SpiceTrade.Core.ValueObjects;

namespace SpiceTrade.Application.Services;

public sealed class TradeService
{
    private readonly IItemRepository _itemRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IPriceCalculator _priceCalculator;

    public TradeService(
        IItemRepository itemRepository,
        ICityRepository cityRepository,
        IPriceCalculator priceCalculator)
    {
        _itemRepository = itemRepository;
        _cityRepository = cityRepository;
        _priceCalculator = priceCalculator;
    }

    public TradeResult Buy(Player player, string itemKey, int quantity, Season season)
    {
        if (quantity <= 0)
            return new TradeResult { Success = false, Message = "Количество должно быть положительным" };

        var item = _itemRepository.Get(itemKey);
        if (item == null)
            return new TradeResult { Success = false, Message = "Товар не найден" };

        var price = _priceCalculator.Calculate(itemKey, player.CurrentCityKey, season);
        var totalCost = price * quantity;

        player.Inventory.Add(itemKey, quantity);
        
        return new TradeResult
        {
            Success = true,
            Message = $"Куплено {quantity} x {item.LocalizationKey} за {totalCost}",
            TotalPrice = totalCost,
            QuantityTraded = quantity
        };
    }

    public TradeResult Sell(Player player, string itemKey, int quantity, Season season)
    {
        if (quantity <= 0)
            return new TradeResult { Success = false, Message = "Количество должно быть положительным" };

        var item = _itemRepository.Get(itemKey);
        if (item == null)
            return new TradeResult { Success = false, Message = "Товар не найден" };

        if (!player.Inventory.TryTake(itemKey, quantity))
            return new TradeResult { Success = false, Message = "Недостаточно товара в инвентаре" };

        var price = _priceCalculator.Calculate(itemKey, player.CurrentCityKey, season);
        var totalRevenue = price * quantity;

        return new TradeResult
        {
            Success = true,
            Message = $"Продано {quantity} x {item.LocalizationKey} за {totalRevenue}",
            TotalPrice = totalRevenue,
            QuantityTraded = quantity
        };
    }
}