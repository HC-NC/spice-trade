using Spectre.Console;
using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Interfaces;

namespace SpiceTrade.CLI.UI.Screens;

public static class TravelScreen
{
    public static string? Show(
        IReadOnlyList<City> cities,
        string currentCityKey,
        Func<string, string> localize)
    {
        var otherCities = cities.Where(c => c.Key != currentCityKey).ToList();

        var table = new Table().Expand();
        table.AddColumn("Город");
        table.AddColumn("Регион");

        foreach (var city in cities)
        {
            var mark = city.Key == currentCityKey ? "✓" : " ";
            table.AddRow($"{mark} {localize(city.LocalizationKey)}", city.Region);
        }

        AnsiConsole.Write(table);

        var destination = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Куда отправиться?[/]")
                .AddChoices(otherCities.Select(c => localize(c.LocalizationKey)).ToList()));

        return otherCities.First(c => localize(c.LocalizationKey) == destination).Key;
    }
}