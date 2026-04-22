using Spectre.Console;
using Spectre.Console.Rendering;
using SpiceTrade.CLI.Services;
using SpiceTrade.CLI.Properties;

namespace SpiceTrade.CLI.UI.Screens;

public class MainMenuScreen : IScreen
{
    private int _selectedIndex = 0;
    private readonly string[] _options = { "Загрузить игру", "Начать игру", "Настройки", "Выход" };

    private FigletFont _smallFont;

    public event Func<IScreen>? OnLoadGameRequested;

    public MainMenuScreen()
    {
        using (var ms = new MemoryStream(Resources.FigletFont_Small))
        {
            _smallFont = FigletFont.Load(ms);
        }
    }

    public IRenderable GetContent()
    {
        IRenderable logo;

        if (AnsiConsole.Console.Profile.Width < 84)
            logo = new FigletText(_smallFont, "SPICE TRADE").Centered().Color(Color.Gold1);
        else
            logo = new FigletText("SPICE TRADE").Centered().Color(Color.Gold1);

        var menuTable = new Table().NoBorder().HideHeaders().AddColumn("Option");

        for (int i = 0; i < _options.Length; i++)
        {
            if (i == _selectedIndex)
                menuTable.AddRow($"[black on white] > {_options[i]} [/]");
            else
                menuTable.AddRow($"  {_options[i]}  ");
        }

        return new Rows(
            logo,
            new Text("\n"),
            Align.Center(new Text("ГЛАВНОЕ МЕНЮ", new Style(Color.Grey))),
            Align.Center(menuTable)
        );
    }

    public IScreen? HandleInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                break;
            case ConsoleKey.DownArrow:
                _selectedIndex = Math.Min(_options.Length - 1, _selectedIndex + 1);
                break;
            case ConsoleKey.Enter:
                return ExecuteSelection();
        }
        return this;
    }

    private IScreen? ExecuteSelection()
    {
        if (_selectedIndex == 0)
        {
            return OnLoadGameRequested?.Invoke();
        }
        if (_selectedIndex == 3) return null;
        return this;
    }
}