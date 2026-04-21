using Spectre.Console;
using SpiceTrade.Application.Services;
using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Enums;
using SpiceTrade.Core.Interfaces;
using SpiceTrade.Core.Services;
using SpiceTrade.Core.ValueObjects;
using SpiceTrade.CLI.Services;
using SpiceTrade.Infrastructure.Providers;
using SpiceTrade.Infrastructure.Repositories;

namespace SpiceTrade.CLI.Game;

public sealed class GameLoop
{
    private readonly JsonDataProvider _dataProvider;
    private readonly ICoinRepository _coinRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IPriceCalculator _priceCalculator;
    private readonly TradeService _tradeService;
    private readonly TravelService _travelService;
    private readonly TimeService _timeService;
    private readonly ILocalizationService _localization;

    private Player _player = null!;
    private GameTime _gameTime = new();

    public GameLoop()
    {
        _dataProvider = new JsonDataProvider();
        _coinRepository = new InMemoryCoinRepository(_dataProvider);
        _itemRepository = new InMemoryItemRepository(_dataProvider);
        _cityRepository = new InMemoryCityRepository(_dataProvider);
        _contractRepository = new InMemoryContractRepository(_dataProvider);
        _priceCalculator = new PriceCalculator(_itemRepository, _cityRepository);
        _tradeService = new TradeService(_itemRepository, _cityRepository, _priceCalculator);
        _travelService = new TravelService(_cityRepository);
        _timeService = new TimeService();
        _localization = new SimpleLocalizationService();

        _tradeService.SetCoinRepository(_coinRepository);
    }

    public void Start()
    {
        InitializePlayer();
        Run();
    }

    private void InitializePlayer()
    {
        var wallet = new Wallet();
        wallet.Add("coin_gold_ducat", 100);

        var inventory = new Inventory();

        _player = new Player
        {
            Name = AnsiConsole.Ask<string>("Ваше имя:"),
            Wallet = wallet,
            Inventory = inventory,
            CurrentCityKey = "city_venice"
        };
    }

    private void Run()
    {
        while (true)
        {
            AnsiConsole.Clear();
            var seasonName = _localization.Get($"ui_season_{_gameTime.Season.ToString().ToLower()}");
            var city = _cityRepository.Get(_player.CurrentCityKey);
            var cityName = city != null ? _localization.Get(city.LocalizationKey) : "?";
            var walletValue = _player.Wallet.GetTotalValue(k => _coinRepository.Get(k));

            ShowMainMenu(cityName, seasonName, walletValue);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Что делать?[/]")
                    .AddChoices("Рынок", "Инвентарь", "Путешествие", "Контракты", "Ждать", "Выход"));

            if (choice == "Выход") break;

            switch (choice)
            {
                case "Рынок": ShowMarket(); break;
                case "Инвентарь": ShowInventory(); break;
                case "Путешествие": ShowTravel(); break;
                case "Контракты": ShowContracts(); break;
                case "Ждать": WaitDays(); break;
            }

            if (choice != "Выход")
                AnsiConsole.Prompt(new TextPrompt<string>("\nEnter для продолжения...").AllowEmpty());
        }

        AnsiConsole.WriteLine("До свидания!");
    }

    private void ShowMainMenu(string cityName, string seasonName, decimal walletValue)
    {
        var panel = new Panel($"[bold]{_player.Name}[/] в [yellow]{cityName}[/]\n{_gameTime.Day}/{_gameTime.Month}/{_gameTime.Year} ({seasonName})\nКошелёк: [green]{walletValue:F2}[/]")
            .Expand()
            .RoundedBorder();
        AnsiConsole.Write(panel);
    }

    private void ShowMarket()
    {
        var items = _itemRepository.GetAll();
        var season = _gameTime.Season;

        AnsiConsole.WriteLine("\n[bold]=== Рынок ===[/]");
        var table = new Table().Expand();
        table.AddColumn("Товар");
        table.AddColumn("Цена");
        table.AddColumn("Категория");

        foreach (var itm in items)
        {
            var price = _priceCalculator.Calculate(itm.LocalizationKey, _player.CurrentCityKey, season);
            table.AddRow(_localization.Get(itm.LocalizationKey), price.ToString("F2"), itm.Category.ToString());
        }

        AnsiConsole.Write(table);

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Что делать?[/]")
                .AddChoices("Купить", "Продать", "Назад"));

        if (action == "Назад") return;

        var itemName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Выберите товар:")
                .AddChoices(items.Select(i => _localization.Get(i.LocalizationKey)).ToList()));

        var item = items.First(i => _localization.Get(i.LocalizationKey) == itemName);
        var quantityStr = AnsiConsole.Ask<string>("Количество:");
        if (!int.TryParse(quantityStr, out var quantity) || quantity <= 0) return;

        if (action == "Купить")
        {
            var price = _priceCalculator.Calculate(item.LocalizationKey, _player.CurrentCityKey, season);
            var total = price * quantity;

            if (_player.Wallet.TryTakeByValue(total, k => _coinRepository.Get(k)))
            {
                _player.Inventory.Add(item.LocalizationKey, quantity);
                AnsiConsole.MarkupLine($"[green]Куплено {quantity} x {_localization.Get(item.LocalizationKey)} за {total:F2}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Недостаточно средств![/]");
            }
        }
        else
        {
            if (_player.Inventory.TryTake(item.LocalizationKey, quantity))
            {
                var price = _priceCalculator.Calculate(item.LocalizationKey, _player.CurrentCityKey, season);
                var total = price * quantity;
                _player.Wallet.Add("coin_gold_ducat", (int)total);
                AnsiConsole.MarkupLine($"[green]Продано {quantity} x {_localization.Get(item.LocalizationKey)} за {total:F2}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Недостаточно товара![/]");
            }
        }
    }

    private void ShowInventory()
    {
        AnsiConsole.WriteLine("\n[bold]=== Инвентарь ===[/]");
        var table = new Table().Expand();
        table.AddColumn("Товар");
        table.AddColumn("Количество");

        var inventory = _player.Inventory.GetAll();
        if (inventory.Count == 0)
        {
            AnsiConsole.WriteLine("Пусто");
            return;
        }

        foreach (var (itemKey, qty) in inventory)
        {
            table.AddRow(_localization.Get(itemKey), qty.ToString());
        }

        AnsiConsole.Write(table);
    }

    private void ShowTravel()
    {
        var cities = _cityRepository.GetAll();
        var otherCities = cities.Where(c => c.Key != _player.CurrentCityKey).ToList();

        AnsiConsole.WriteLine("\n[bold]=== Путешествие ===[/]");
        var table = new Table().Expand();
        table.AddColumn("Город");
        table.AddColumn("Регион");

        foreach (var city in cities)
        {
            var mark = city.Key == _player.CurrentCityKey ? "[green]✓[/]" : " ";
            table.AddRow($"{mark} {_localization.Get(city.LocalizationKey)}", city.Region);
        }

        AnsiConsole.Write(table);

        if (otherCities.Count == 0) return;

        var destinationName = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Куда отправиться?[/]")
                .AddChoices(otherCities.Select(c => _localization.Get(c.LocalizationKey)).ToList()));

        var destination = otherCities.First(c => _localization.Get(c.LocalizationKey) == destinationName);
        var result = _travelService.Travel(_player, destination.Key, _gameTime);

        AnsiConsole.MarkupLine($"[yellow]{result.Message}[/]");
    }

    private void ShowContracts()
    {
        var contracts = _contractRepository.GetByCity(_player.CurrentCityKey);
        
        AnsiConsole.WriteLine("\n[bold]=== Контракты ===[/]");
        
        if (contracts.Count == 0)
        {
            AnsiConsole.WriteLine("Контрактов нет.");
            return;
        }

        var table = new Table().Expand();
        table.AddColumn("Контракт");
        table.AddColumn("Товар");
        table.AddColumn("Маршрут");
        table.AddColumn("Срок");

        foreach (var contract in contracts)
        {
            var origin = _localization.Get(_cityRepository.Get(contract.OriginCityKey)?.LocalizationKey ?? "");
            var dest = _localization.Get(_cityRepository.Get(contract.DestinationCityKey)?.LocalizationKey ?? "");
            table.AddRow(
                _localization.Get(contract.LocalizationKey),
                _localization.Get(contract.ItemKey),
                $"{origin} → {dest}",
                $"{contract.DeadlineDays} дн.");
        }

        AnsiConsole.Write(table);
    }

    private void WaitDays()
    {
        var daysStr = AnsiConsole.Ask<string>("Сколько дней ждать:");
        if (!int.TryParse(daysStr, out var days) || days <= 0) return;

        var result = _timeService.Wait(_gameTime, days);
        AnsiConsole.MarkupLine($"[yellow]{result.Message}[/]");
    }
}