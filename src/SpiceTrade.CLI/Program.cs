using Spectre.Console;
using SpiceTrade.CLI.Game;

namespace SpiceTrade.CLI;

class Program
{
    private static void Main(string[] args)
    {
        var game = new GameLoop();
        game.Start();
    }
}