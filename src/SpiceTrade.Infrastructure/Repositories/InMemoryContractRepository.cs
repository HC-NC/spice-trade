using SpiceTrade.Core.Entities;
using SpiceTrade.Core.Enums;
using SpiceTrade.Core.Interfaces;
using SpiceTrade.Infrastructure.Data;
using SpiceTrade.Infrastructure.Providers;

namespace SpiceTrade.Infrastructure.Repositories;

public sealed class InMemoryContractRepository : IContractRepository
{
    private readonly Dictionary<string, Contract> _contracts;

    public InMemoryContractRepository(JsonDataProvider dataProvider)
    {
        var data = dataProvider.Load();
        _contracts = data.Contracts.ToDictionary(c => c.Key, c => new Contract
        {
            LocalizationKey = c.LocalizationKey,
            ItemKey = c.ItemKey,
            Quantity = c.Quantity,
            OriginCityKey = c.OriginCityKey,
            DestinationCityKey = c.DestinationCityKey,
            DeadlineDays = c.DeadlineDays,
            PaymentType = Enum.Parse<ContractPaymentType>(c.PaymentType),
            PaymentAmount = c.PaymentAmount
        });
    }

    public Contract? Get(string contractKey) => _contracts.GetValueOrDefault(contractKey);

    public IReadOnlyList<Contract> GetAll() => _contracts.Values.ToList();

    public IReadOnlyList<Contract> GetByCity(string cityKey) =>
        _contracts.Values.Where(c => c.OriginCityKey == cityKey || c.DestinationCityKey == cityKey).ToList();
}