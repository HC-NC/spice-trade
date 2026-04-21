using Spectre.Console;
using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Interfaces;
using SpiceTrade.Core.ValueObjects;

namespace SpiceTrade.CLI.UI.Screens;

public static class MarketScreen
{
    public static void Show(
        IReadOnlyList<Item> items,
        IReadOnlyList<City> cities,
        Func<string, string, int, decimal> getPrice,
        Func<string, string> localize,
        Func<string, decimal> getWalletValue,
        Action<string, int, bool> trade)
    {
        var table = new Table().Expand();
        table.AddColumn("Товар");
        table.AddColumn("Цена");
        table.AddColumn("Категория");

        foreach (var item in items)
        {
            var price = getPrice(item.LocalizationKey, cities.First().LocalizationKey, 0);
            table.AddRow(localize(item.LocalizationKey), price.ToString("F2"), item.Category.ToString());
        }

        AnsiConsole.Write(table);

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Что делать?[/]")
                .AddChoices(new[] { "Купить", "Продать", "Назад" }));

        if (action == "Назад") return;

        var selectedItem = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Выберите товар:")
                .AddChoices(items.Select(i => localize(i.LocalizationKey)).ToList()));

        var quantityStr = AnsiConsole.Ask<string>("Количество:");
        if (!int.TryParse(quantityStr, out var quantity)) return;

        var itemKey = items.First(i => localize(i.LocalizationKey) == selectedItem).LocalizationKey;
        
        if (action == "Купить")
            trade(itemKey, quantity, true);
        else
            trade(itemKey, quantity, false);
    }
}