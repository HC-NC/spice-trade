using SpiceTrade.Core.Entities;

namespace SpiceTrade.Core.Interfaces;

public interface IContractRepository
{
    Contract? Get(string contractKey);
    IReadOnlyList<Contract> GetAll();
    IReadOnlyList<Contract> GetByCity(string cityKey);
}