using SpiceTrade.Core.Enums;

namespace SpiceTrade.Core.Entities;

public sealed class Contract
{
    public required string LocalizationKey { get; init; }
    public required string ItemKey { get; init; }
    public required int Quantity { get; init; }
    public required string OriginCityKey { get; init; }
    public required string DestinationCityKey { get; init; }
    public required int DeadlineDays { get; init; }
    public required ContractPaymentType PaymentType { get; init; }
    public required decimal PaymentAmount { get; init; }
}