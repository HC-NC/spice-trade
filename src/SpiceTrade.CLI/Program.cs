using Spectre.Console;
using SpiceTrade.CLI.Game;

namespace SpiceTrade.CLI;

class Program
{
    private static void Main(string[] args)
    {
        AnsiConsole.MarkupLine("[bold yellow]Spice Trade[/]");
        AnsiConsole.WriteLine();

        var game = new GameLoop();
        game.Start();
    }
}