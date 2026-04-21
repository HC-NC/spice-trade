using Spectre.Console;
using SpiceTrade.Application.Services;
using SpiceTrade.Core;
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
    private readonly ICoinTypeRepository _coinTypeRepository;
    private readonly ICoinRepository _coinRepository;
    private readonly IItemRepository _itemRepository;
    private readonly ICityRepository _cityRepository;
    private readonly IContractRepository _contractRepository;
    private readonly IPriceCalculator _priceCalculator;
    private readonly ICoinValueService _coinValueService;
    private readonly TradeService _tradeService;
    private readonly TravelService _travelService;
    private readonly TimeService _timeService;
    private readonly ILocalizationService _localization;
    private readonly ISaveService _saveService;
    private readonly string _savePath = "save.trp";

    private Player _player = null!;
    private GameTime _gameTime = new();

    public GameLoop()
    {
        _dataProvider = new JsonDataProvider();
        _coinTypeRepository = new InMemoryCoinTypeRepository(_dataProvider);
        _coinRepository = new InMemoryCoinRepository();
        _itemRepository = new InMemoryItemRepository(_dataProvider);
        _cityRepository = new InMemoryCityRepository(_dataProvider);
        _contractRepository = new InMemoryContractRepository(_dataProvider);
        _priceCalculator = new PriceCalculator(_itemRepository, _cityRepository);
        var metalPrices = new SimpleMetalPriceProvider();
        _coinValueService = new SimpleCoinValueService(_coinTypeRepository, metalPrices);
        _tradeService = new TradeService(_itemRepository, _cityRepository, _priceCalculator);
        _travelService = new TravelService(_cityRepository);
        _timeService = new TimeService();
        _localization = new SimpleLocalizationService();
        _saveService = new ZipSaveService();
    }

    public void Start()
    {
        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold yellow]Spice Trade[/]")
                .AddChoices("Новая игра", "Загрузить игру", "Выход"));

        if (choice == "Выход") return;
        
        if (choice == "Загрузить игру")
        {
            if (!LoadGame())
            {
                AnsiConsole.MarkupLine("[red]Нет сохранений или ошибка загрузки![/]");
                AnsiConsole.Prompt(new TextPrompt<string>("[Enter для продолжения...]").AllowEmpty());
                Start();
                return;
            }
        }
        else
        {
            InitializePlayer();
        }
        
        Run();
    }

    private bool LoadGame()
    {
        var state = _saveService.Load<GameState>(_savePath);
        if (state == null) return false;

        _gameTime = new GameTime();
        while (_gameTime.Year < state.Year || _gameTime.Month < state.Month || _gameTime.Day < state.Day)
        {
            _gameTime.Advance(1);
        }

        var wallet = new Wallet { Name = "Кошелёк", Capacity = 500 };
        foreach (var coinState in state.Coins)
        {
            var coin = _coinRepository.Create(coinState.CoinTypeKey, coinState.Year);
            coin.Properties = coinState.Properties;
            wallet.Add(coin);
        }

        var inventory = new Inventory();
        foreach (var (itemKey, qty) in state.Inventory)
        {
            inventory.Add(itemKey, qty);
        }

        _player = new Player
        {
            Name = state.PlayerName,
            Wallet = wallet,
            Inventory = inventory,
            CurrentCityKey = state.CurrentCityKey
        };

        return true;
    }

    private void SaveGame()
    {
        var state = new GameState
        {
            PlayerName = _player.Name,
            CurrentCityKey = _player.CurrentCityKey,
            Day = _gameTime.Day,
            Month = _gameTime.Month,
            Year = _gameTime.Year,
            Inventory = new Dictionary<string, int>(_player.Inventory.GetAll()),
            Coins = _player.Wallet.Coins.Select(c => new CoinState
            {
                CoinTypeKey = c.CoinTypeKey,
                Year = c.Year,
                Condition = c.Condition,
                Properties = c.Properties
            }).ToList()
        };

        _saveService.Save(_savePath, state);
        AnsiConsole.MarkupLine("[green]Игра сохранена![/]");
    }

    private void InitializePlayer()
    {
        var wallet = new Wallet { Name = "Кошелёк", Capacity = 500 };
        for (int i = 0; i < 10; i++)
        {
            var coin = _coinRepository.Create("coin_gold_ducat", _gameTime.Year);
            wallet.Add(coin);
        }

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
            var region = city?.Region ?? "?";
            var walletValue = _coinValueService.GetTotalValue(_player.Wallet.Coins, region);

            ShowMainMenu(cityName, seasonName, walletValue);

            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Что делать?[/]")
                    .AddChoices("Рынок", "Инвентарь", "Кошелёк", "Путешествие", "Контракты", "Ждать", "Сохранить", "Выход"));

            if (choice == "Выход") break;

            switch (choice)
            {
                case "Рынок": ShowMarket(); break;
                case "Инвентарь": ShowInventory(); break;
                case "Кошелёк": ShowWallet(); break;
                case "Путешествие": ShowTravel(); break;
                case "Контракты": ShowContracts(); break;
                case "Ждать": WaitDays(); break;
                case "Сохранить": SaveGame(); break;
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
            var city = _cityRepository.Get(_player.CurrentCityKey);
            var region = city?.Region ?? "Италия";

            if (TryTakeFromWallet(total, region))
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
                AddToWallet(total, region: "Италия");
                AnsiConsole.MarkupLine($"[green]Продано {quantity} x {_localization.Get(item.LocalizationKey)} за {total:F2}[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Недостаточно товара![/]");
            }
        }
    }

    private bool TryTakeFromWallet(decimal amount, string region)
    {
        var availableCoins = _player.Wallet.Coins.ToList();
        var totalValue = _coinValueService.GetTotalValue(availableCoins, region);

        if (totalValue < amount) return false;

        var sortedCoins = availableCoins
            .OrderByDescending(c => _coinValueService.GetNominalValue(c, region))
            .ToList();

        var toTake = new List<Coin>();
        var sum = 0m;
        
        foreach (var coin in sortedCoins)
        {
            var coinValue = _coinValueService.GetNominalValue(coin, region);
            if (sum + coinValue <= amount * 1.5m)
            {
                toTake.Add(coin);
                sum += coinValue;
                if (sum >= amount) break;
            }
        }

        if (sum < amount) return false;

        foreach (var coin in toTake)
        {
            _player.Wallet.TryTake(c => c.Id == coin.Id, out _);
        }

        var change = sum - amount;
        if (change > 0.01m)
        {
            GiveChange(change, region);
        }

        return true;
    }

    private void GiveChange(decimal change, string region)
    {
        var coinTypes = _coinTypeRepository.GetAll()
            .OrderBy(c => c.BaseDenomination)
            .ToList();

        foreach (var coinType in coinTypes)
        {
            while (change >= coinType.BaseDenomination * 0.9m && _player.Wallet.CanAdd())
            {
                var coin = _coinRepository.Create(coinType.Key, _gameTime.Year);
                _player.Wallet.Add(coin);
                change -= coinType.BaseDenomination;
            }
        }
        
        if (change > 0.01m)
        {
            AnsiConsole.MarkupLine($"[yellow]Внимание: сдача {change:F2} не выдана (нет мелких монет)[/]");
        }
    }

    private void AddToWallet(decimal amount, string region)
    {
        var coinTypes = _coinTypeRepository.GetAll().OrderByDescending(c => c.BaseDenomination).ToList();

        foreach (var coinType in coinTypes)
        {
            while (amount >= coinType.BaseDenomination && _player.Wallet.CanAdd())
            {
                var coin = _coinRepository.Create(coinType.Key, _gameTime.Year);
                _player.Wallet.Add(coin);
                amount -= coinType.BaseDenomination;
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

    private void ShowWallet()
    {
        var city = _cityRepository.Get(_player.CurrentCityKey);
        var region = city?.Region ?? "?";

        AnsiConsole.WriteLine("\n[bold]=== Кошелёк ===[/]");
        
        var grouped = _player.Wallet.Coins
            .GroupBy(c => c.CoinTypeKey)
            .Select(g => new {
                CoinTypeKey = g.Key,
                Count = g.Count(),
                TotalNominal = g.Sum(c => _coinValueService.GetNominalValue(c, region)),
                TotalMetal = g.Sum(c => _coinValueService.GetMetalValue(c))
            })
            .ToList();

        var table = new Table().Expand();
        table.AddColumn("Монета");
        table.AddColumn("Кол-во");
        table.AddColumn("Номинал");
        table.AddColumn("Металл");

        foreach (var group in grouped)
        {
            var coinType = _coinTypeRepository.Get(group.CoinTypeKey);
            var name = _localization.Get(coinType?.LocalizationKey ?? group.CoinTypeKey);
            table.AddRow(name, group.Count.ToString(), group.TotalNominal.ToString("F2"), group.TotalMetal.ToString("F2"));
        }

        AnsiConsole.Write(table);

        var totalValue = _coinValueService.GetTotalValue(_player.Wallet.Coins, region);
        AnsiConsole.MarkupLine($"\n[bold]Итого:[/] [green]{totalValue:F2}[/]");

        var action = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold]Операции?[/]")
                .AddChoices("Размен", "Слияние", "Назад"));

        if (action == "Размен")
            ShowExchange();
        else if (action == "Слияние")
            ShowMerge();
    }

    private void ShowExchange()
    {
        AnsiConsole.WriteLine("\n[bold]=== Размен ===[/]");
        
        var coinTypes = _coinTypeRepository.GetAll();
        var fromType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Какую монету разменять:")
                .AddChoices(coinTypes.Select(c => _localization.Get(c.LocalizationKey)).ToList()));

        var selectedType = coinTypes.First(c => _localization.Get(c.LocalizationKey) == fromType);
        
        var hasCoins = _player.Wallet.Coins.Any(c => c.CoinTypeKey == selectedType.Key);
        if (!hasCoins)
        {
            AnsiConsole.MarkupLine("[red]У вас нет таких монет![/]");
            return;
        }

        var targetTypes = coinTypes.Where(c => c.BaseDenomination < selectedType.BaseDenomination).ToList();
        if (targetTypes.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Нет монет для размена (нет мельче)[/]");
            return;
        }

        var toType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("На какую монету:")
                .AddChoices(targetTypes.Select(c => _localization.Get(c.LocalizationKey)).ToList()));

        var targetCoinType = targetTypes.First(c => _localization.Get(c.LocalizationKey) == toType);
        
        var ratio = (int)(selectedType.BaseDenomination / targetCoinType.BaseDenomination);
        var amountStr = AnsiConsole.Ask<string>($"Сколько {fromType} разменять (1 = {ratio} {toType}):");
        
        if (!int.TryParse(amountStr, out var amount) || amount <= 0) return;

        var coinsToExchange = _player.Wallet.Coins.Where(c => c.CoinTypeKey == selectedType.Key).Take(amount).ToList();
        
        if (coinsToExchange.Count < amount)
        {
            AnsiConsole.MarkupLine("[red]Недостаточно монет![/]");
            return;
        }

        var exchanged = 0;
        foreach (var coin in coinsToExchange)
        {
            _player.Wallet.TryTake(c => c.Id == coin.Id, out _);
            for (int i = 0; i < ratio; i++)
            {
                if (_player.Wallet.CanAdd())
                {
                    var newCoin = _coinRepository.Create(targetCoinType.Key, _gameTime.Year);
                    _player.Wallet.Add(newCoin);
                    exchanged++;
                }
            }
        }

        var expected = amount * ratio;
        if (exchanged < expected)
        {
            AnsiConsole.MarkupLine($"[yellow]Внимание: кошелёк переполнен! Обменяно {exchanged}/{expected} монет[/]");
        }
        else
        {
            AnsiConsole.MarkupLine($"[green]Разменено {amount} {fromType} на {exchanged} {toType}[/]");
        }
    }

    private void ShowMerge()
    {
        AnsiConsole.WriteLine("\n[bold]=== Слияние ===[/]");

        var grouped = _player.Wallet.Coins
            .GroupBy(c => c.CoinTypeKey)
            .Where(g => g.Count() > 1)
            .ToList();

        if (grouped.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Нечего объединять (нет повторяющихся монет)[/]");
            return;
        }

        var coinTypes = _coinTypeRepository.GetAll();
        
        foreach (var group in grouped)
        {
            var coinType = coinTypes.FirstOrDefault(c => c.Key == group.Key);
            var name = _localization.Get(coinType?.LocalizationKey ?? group.Key);
            AnsiConsole.MarkupLine($"[green]{name}[/]: {group.Count()} шт.");
        }

        var toMerge = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Какие монеты объединить:")
                .AddChoices(grouped.Select(g => {
                    var ct = coinTypes.FirstOrDefault(c => c.Key == g.Key);
                    return _localization.Get(ct?.LocalizationKey ?? g.Key);
                }).ToList()));

        var selectedGroup = grouped.First(g => {
            var ct = coinTypes.FirstOrDefault(c => c.Key == g.Key);
            return _localization.Get(ct?.LocalizationKey ?? g.Key) == toMerge;
        });

        var mergeCountStr = AnsiConsole.Ask<string>("Сколько монет объединить в стопку:");
        if (!int.TryParse(mergeCountStr, out var mergeCount) || mergeCount < 2) return;
        if (mergeCount > selectedGroup.Count()) mergeCount = selectedGroup.Count();

        AnsiConsole.MarkupLine($"[green]Объединено {mergeCount} монет в стопку[/]");
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