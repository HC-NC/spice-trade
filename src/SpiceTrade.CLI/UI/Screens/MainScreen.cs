using Spectre.Console;
using Spectre.Console.Rendering;
using SpiceTrade.Core.Entities;

namespace SpiceTrade.CLI.UI.Screens
{
    public class MainScreen : IScreen
    {
        private readonly Player _player;
        private readonly City _city;
        private int _menuIndex = 0;
        private readonly string[] _cityActions = { "Рынок", "Контракты", "Путешествие", "Отдых", "Сохранить" };

        public MainScreen(Player player, City city)
        {
            _player = player;
            _city = city;
        }

        public IRenderable GetContent()
        {
            // Создаем внутренний Layout для главного экрана
            var mainLayout = new Layout("Dashboard")
                .SplitColumns(
                    new Layout("Actions").Ratio(2),
                    new Layout("Status").Ratio(1)
                );

            // 1. Левая часть: Меню действий
            var menuTable = new Table().NoBorder().HideHeaders().AddColumn("Op");
            for (int i = 0; i < _cityActions.Length; i++)
            {
                string style = (i == _menuIndex) ? "bold gold1 reverse" : "white";
                menuTable.AddRow($"[{style}]  {_cityActions[i]}  [/]");
            }

            mainLayout["Actions"].Update(
                new Panel(menuTable)
                    .Header($"ГОРOД: {_city.Key.ToUpper()}")
                    .Expand()
            );

            Panel minimap = new Panel(new Text("Здесь будет\nмини-карта")).Header("Карта");
            minimap.Height = 7;

            // 2. Правая часть: Стаки виджетов
            var statusWidgets = new Rows(
                minimap,
                new Panel(RenderMiniWallet()).Header("Кошелек"),
                new Panel(RenderWeightBar()).Header("Груз")
            );

            mainLayout["Status"].Update(statusWidgets);

            return mainLayout;
        }

        private IRenderable RenderMiniWallet() { return new Text(""); }
        private IRenderable RenderWeightBar() { return new Text(""); }

        public IScreen? HandleInput(ConsoleKeyInfo key)
        {
            switch (key.Key)
            {
                case ConsoleKey.UpArrow:
                    _menuIndex = Math.Max(0, _menuIndex - 1);
                    break;
                case ConsoleKey.DownArrow:
                    _menuIndex = Math.Min(_cityActions.Length - 1, _menuIndex + 1);
                    break;
            }
            return this;
        }
    }
}
