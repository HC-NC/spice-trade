using Spectre.Console;
using SpiceTrade.CLI.Services;

namespace SpiceTrade.CLI.UI.Screens;

public static class MainMenuScreen
{
    public static int Show(string playerName, string cityName, int day, int month, int year, string season, decimal walletValue)
    {
        var panel = new Panel($"[bold]{playerName}[/] в [yellow]{cityName}[/]\n{day}/{month}/{year} ({season})\nКошелёк: [green]{walletValue}[/]")
            .Expand()
            .RoundedBorder();

        AnsiConsole.Write(panel);

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Что делать?[/]")
                .AddChoices(new[] { "Рынок", "Инвентарь", "Путешествие", "Контракты", "Ждать", "Выход" }));

        return choice switch
        {
            "Рынок" => 1,
            "Инвентарь" => 2,
            "Путешествие" => 3,
            "Контракты" => 4,
            "Ждать" => 5,
            "Выход" => 0,
            _ => 0
        };
    }
}