using Spectre.Console;
using Spectre.Console.Rendering;
using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Enums;
using SpiceTrade.Core.Interfaces;
using SpiceTrade.Core.ValueObjects;

namespace SpiceTrade.CLI.UI.Screens;

public class MarketScreen
{
    private readonly IItemRepository _itemRepository;
    private readonly ICityRepository _cityRepository;
    private readonly ICoinTypeRepository _coinTypeRepository;
    private readonly IPriceCalculator _priceCalculator;
    private readonly ICoinValueService _coinValueService;
    private readonly ILocalizationService _localization;

    public MarketScreen(
        IItemRepository itemRepository,
        ICityRepository cityRepository,
        ICoinTypeRepository coinTypeRepository,
        IPriceCalculator priceCalculator,
        ICoinValueService coinValueService,
        ILocalizationService localization)
    {
        _itemRepository = itemRepository;
        _cityRepository = cityRepository;
        _coinTypeRepository = coinTypeRepository;
        _priceCalculator = priceCalculator;
        _coinValueService = coinValueService;
        _localization = localization;
    }

    public void Show(Player player, GameTime gameTime)
    {
        var city = _cityRepository.Get(player.CurrentCityKey);
        var region = city?.Region ?? "Италия";

        while (true)
        {
            var season = gameTime.Season;
            var items = _itemRepository.GetAll();
            var playerInventory = player.Inventory.GetAll();

            var cityName = _localization.Get(city?.LocalizationKey ?? "?");
            var seasonName = _localization.Get($"ui_season_{season.ToString().ToLower()}");
            var headerText = $"[bold]{_localization.Get("ui_market")}[/]  |  [yellow]{cityName}[/]  |  [dim]{gameTime.Day}/{gameTime.Month}/{gameTime.Year} ({seasonName})[/]";

            AnsiConsole.Write(new Panel(headerText).Expand().NoBorder());
            AnsiConsole.WriteLine();

            var grid = new Grid().AddColumn();
            grid.AddRow(CreatePlayerInventoryPanel(items, playerInventory));
            grid.AddRow(CreateMerchantGoodsPanel(items, player.CurrentCityKey, season));
            AnsiConsole.Write(grid);
            AnsiConsole.WriteLine();

            var footer = $"[dim]←[/{_localization.Get("ui_sell")}]  [dim]→[/{_localization.Get("ui_buy")}]  [dim]ESC[/{_localization.Get("ui_back")}][/]";
            Layout l = new Layout();
            AnsiConsole.Write(new Panel(footer));
            AnsiConsole.WriteLine();

            var action = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title(_localization.Get("ui_choose_action"))
                    .AddChoices(_localization.Get("ui_sell"), _localization.Get("ui_buy"), _localization.Get("ui_back")));

            if (action == _localization.Get("ui_back"))
                return;

            if (action == _localization.Get("ui_sell"))
            {
                ShowSellDialog(player, items, playerInventory, season, region);
            }
            else if (action == _localization.Get("ui_buy"))
            {
                ShowBuyDialog(player, items, season, region);
            }
        }
    }

    private Panel CreatePlayerInventoryPanel(IReadOnlyList<Item> items, IReadOnlyDictionary<string, int> inventory)
    {
        var table = new Table().Expand().RoundedBorder();
        table.AddColumn(new TableColumn(_localization.Get("ui_item")));
        table.AddColumn(new TableColumn("Qty").Width(5).RightAligned());

        foreach (var (itemKey, qty) in inventory.OrderBy(x => x.Key))
        {
            var item = items.FirstOrDefault(i => i.LocalizationKey == itemKey);
            var name = item != null ? _localization.Get(item.LocalizationKey) : itemKey;
            table.AddRow(name, qty.ToString());
        }

        var panel = new Panel(table).Expand();
        panel.Header(new PanelHeader($"[bold]{_localization.Get("ui_your_inventory")}[/]"));
        return panel.RoundedBorder();
    }

    private Panel CreateMerchantGoodsPanel(IReadOnlyList<Item> items, string cityKey, Season season)
    {
        var table = new Table().Expand().RoundedBorder();
        table.AddColumn(new TableColumn(_localization.Get("ui_item")));
        table.AddColumn(new TableColumn(_localization.Get("ui_price")).Width(8).RightAligned());

        foreach (var item in items)
        {
            var price = _priceCalculator.Calculate(item.LocalizationKey, cityKey, season);
            var name = _localization.Get(item.LocalizationKey);
            table.AddRow(name, price.ToString("F2"));
        }

        var panel = new Panel(table).Expand();
        panel.Header(new PanelHeader($"[bold]{_localization.Get("ui_merchant")}[/]"));
        return panel.RoundedBorder();
    }

    private void ShowSellDialog(Player player, IReadOnlyList<Item> items, IReadOnlyDictionary<string, int> inventory, Season season, string region)
    {
        if (inventory.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]{_localization.Get("ui_inventory_empty")}[/]");
            AnsiConsole.Prompt(new TextPrompt<string>("\n[dim]Enter...[/]").AllowEmpty());
            return;
        }

        var playerItems = inventory.Keys
            .Select(k => items.First(i => i.LocalizationKey == k))
            .Select(i => _localization.Get(i.LocalizationKey))
            .ToList();

        var selectedItem = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_localization.Get("ui_sell_what"))
                .AddChoices(playerItems));

        var item = items.First(i => _localization.Get(i.LocalizationKey) == selectedItem);
        var maxQty = inventory.GetValueOrDefault(item.LocalizationKey, 0);

        AnsiConsole.MarkupLine($"[dim]( {_localization.Get("ui_available")}: {maxQty} )[/]");

        var qtyStr = AnsiConsole.Ask<string>(_localization.Get("ui_quantity") + ":");
        if (!int.TryParse(qtyStr, out var qty) || qty <= 0 || qty > maxQty)
        {
            AnsiConsole.MarkupLine("[red]Неверное количество![/]");
            AnsiConsole.Prompt(new TextPrompt<string>("\n[dim]Enter...[/]").AllowEmpty());
            return;
        }

        if (player.Inventory.TryTake(item.LocalizationKey, qty))
        {
            var price = _priceCalculator.Calculate(item.LocalizationKey, player.CurrentCityKey, season);
            var total = price * qty;
            AddCoinsToWallet(player.Wallet, total, region);
            AnsiConsole.MarkupLine($"[green]{_localization.Get("ui_sold")} {qty} x {selectedItem} {_localization.Get("ui_for")} {total:F2}[/]");
        }

        AnsiConsole.Prompt(new TextPrompt<string>("\n[dim]Enter...[/]").AllowEmpty());
    }

    private void ShowBuyDialog(Player player, IReadOnlyList<Item> items, Season season, string region)
    {
        var allKeys = items.Select(i => _localization.Get(i.LocalizationKey)).ToList();

        var selectedItem = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title(_localization.Get("ui_buy_what"))
                .AddChoices(allKeys));

        var item = items.First(i => _localization.Get(i.LocalizationKey) == selectedItem);
        var price = _priceCalculator.Calculate(item.LocalizationKey, player.CurrentCityKey, season);

        AnsiConsole.MarkupLine($"[dim]( {_localization.Get("ui_price")}: {price:F2} )[/]");

        var qtyStr = AnsiConsole.Ask<string>(_localization.Get("ui_quantity") + ":");
        if (!int.TryParse(qtyStr, out var qty) || qty <= 0)
        {
            AnsiConsole.MarkupLine("[red]Неверное количество![/]");
            AnsiConsole.Prompt(new TextPrompt<string>("\n[dim]Enter...[/]").AllowEmpty());
            return;
        }

        var total = price * qty;
        if (TryTakeCoinsFromWallet(player.Wallet, total, region))
        {
            player.Inventory.Add(item.LocalizationKey, qty);
            AnsiConsole.MarkupLine($"[green]{_localization.Get("ui_bought")} {qty} x {selectedItem} {_localization.Get("ui_for")} {total:F2}[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[red]{_localization.Get("ui_not_enough_money")}[/]");
        }

        AnsiConsole.Prompt(new TextPrompt<string>("\n[dim]Enter...[/]").AllowEmpty());
    }

    private bool TryTakeCoinsFromWallet(IMoneyContainer wallet, decimal amount, string region)
    {
        var stacks = wallet.Stacks;
        var totalValue = 0m;
        
        foreach (var (coinTypeKey, count) in stacks)
        {
            var sampleCoin = new Coin { CoinTypeKey = coinTypeKey, Year = 1240, Condition = 100 };
            totalValue += _coinValueService.GetNominalValue(sampleCoin, region) * count;
        }

        if (totalValue < amount) return false;

        var sortedStacks = stacks.OrderByDescending(kv => 
        {
            var sample = new Coin { CoinTypeKey = kv.Key, Year = 1240, Condition = 100 };
            return _coinValueService.GetNominalValue(sample, region);
        }).ToList();

        var toTake = new Dictionary<string, int>();
        var sum = 0m;
        
        foreach (var (coinTypeKey, count) in sortedStacks)
        {
            var sampleCoin = new Coin { CoinTypeKey = coinTypeKey, Year = 1240, Condition = 100 };
            var coinValue = _coinValueService.GetNominalValue(sampleCoin, region);
            var needed = (int)Math.Ceiling((amount - sum) / coinValue);
            var take = Math.Min(count, needed);
            
            if (take > 0)
            {
                toTake[coinTypeKey] = take;
                sum += coinValue * take;
            }
            
            if (sum >= amount) break;
        }

        if (sum < amount) return false;

        foreach (var (coinTypeKey, count) in toTake)
        {
            wallet.TryTakeCoins(coinTypeKey, count);
        }

        var change = sum - amount;
        if (change > 0.01m)
        {
            GiveChange(wallet, change, region);
        }

        return true;
    }

    private void GiveChange(IMoneyContainer wallet, decimal change, string region)
    {
        var coinTypes = _coinTypeRepository.GetAll()
            .OrderBy(c => c.BaseDenomination)
            .ToList();

        foreach (var coinType in coinTypes)
        {
            while (change >= coinType.BaseDenomination * 0.9m && wallet.CanAdd(1))
            {
                wallet.AddCoins(coinType.Key, 1, 1240);
                change -= coinType.BaseDenomination;
            }
        }
        
        if (change > 0.01m)
        {
            AnsiConsole.MarkupLine($"[yellow]Сдача {change:F2} не выдана[/]");
        }
    }

    private void AddCoinsToWallet(IMoneyContainer wallet, decimal amount, string region)
    {
        var coinTypes = _coinTypeRepository.GetAll().OrderByDescending(c => c.BaseDenomination).ToList();

        foreach (var coinType in coinTypes)
        {
            while (amount >= coinType.BaseDenomination && wallet.CanAdd(1))
            {
                wallet.AddCoins(coinType.Key, 1, 1240);
                amount -= coinType.BaseDenomination;
            }
        }
    }
}