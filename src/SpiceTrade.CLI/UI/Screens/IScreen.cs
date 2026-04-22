using Spectre.Console.Rendering;

namespace SpiceTrade.CLI.UI.Screens
{
    public interface IScreen
    {
        IRenderable GetContent();
        IScreen? HandleInput(ConsoleKeyInfo key);
    }
}
