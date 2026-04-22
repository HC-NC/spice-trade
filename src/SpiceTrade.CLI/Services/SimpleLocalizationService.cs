using SpiceTrade.Core.Interfaces;

namespace SpiceTrade.CLI.Services;

public sealed class SimpleLocalizationService : ILocalizationService
{
    private readonly Dictionary<string, string> _translations = new()
    {
        // Coins
        ["coin_gold_ducat"] = "Золотой дукат",
        ["coin_silver_groshen"] = "Серебряный грош",
        ["coin_copper_denari"] = "Медный денарий",
        
        // Items
        ["item_wheat"] = "Пшеница",
        ["item_salt"] = "Соль",
        ["item_pepper"] = "Перец",
        ["item_silk"] = "Шёлк",
        ["item_iron"] = "Железо",
        
        // Cities
        ["city_venice"] = "Венеция",
        ["city_constantinople"] = "Константинополь",
        ["city_genoa"] = "Генуя",
        
        // Contracts
        ["contract_silk_venice_const"] = "Доставка шёлка в Константинополь",
        
        // UI
        ["ui_market_title"] = "Рынок",
        ["ui_inventory_title"] = "Инвентарь",
        ["ui_travel_title"] = "Путешествие",
        ["ui_contracts_title"] = "Контракты",
        ["ui_wallet_title"] = "Кошелёк",
        ["ui_main_menu"] = "Главное меню",
        ["ui_buy"] = "Купить",
        ["ui_sell"] = "Продать",
        ["ui_travel"] = "Путешествовать",
        ["ui_wait"] = "Ждать",
        ["ui_contracts"] = "Контракты",
        ["ui_exit"] = "Выход",
        ["ui_day"] = "День",
        ["ui_month"] = "Месяц",
        ["ui_year"] = "Год",
        ["ui_season_spring"] = "Весна",
        ["ui_season_summer"] = "Лето",
        ["ui_season_autumn"] = "Осень",
        ["ui_season_winter"] = "Зима",
        
        // Market UI
        ["ui_market"] = "Рынок",
        ["ui_your_inventory"] = "Ваш инвентарь",
        ["ui_merchant"] = "Торговец",
        ["ui_merchant_goods"] = "Товары",
        ["ui_item"] = "Товар",
        ["ui_price"] = "Цена",
        ["ui_quantity"] = "Количество",
        ["ui_choose_action"] = "Выберите действие",
        ["ui_sell_what"] = "Что продать",
        ["ui_buy_what"] = "Что купить",
        ["ui_back"] = "Назад",
        ["ui_inventory_empty"] = "Инвентарь пуст",
        ["ui_available"] = "Доступно",
        ["ui_sold"] = "Продано",
        ["ui_bought"] = "Куплено",
        ["ui_for"] = "за",
        ["ui_not_enough_money"] = "Недостаточно денег!",
        ["ui_not_enough_items"] = "Недостаточно товара!"
    };

    public string Get(string key) => _translations.GetValueOrDefault(key, key);

    public string Get(string key, params object[] args)
    {
        var template = Get(key);
        return string.Format(template, args);
    }
}