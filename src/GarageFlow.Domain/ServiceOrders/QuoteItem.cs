using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.ServiceOrders;

public sealed class QuoteItem
{
    public Guid Id { get; private set; }
    public Guid ServiceId { get; private set; }
    public string ServiceName { get; private set; } = string.Empty;
    public decimal LaborPrice { get; private set; }
    public decimal PartsTotal { get; private set; }
    public decimal SuppliesTotal { get; private set; }
    public decimal Subtotal { get; private set; }

    private QuoteItem() { }

    public static QuoteItem Create(
        Guid serviceId,
        string serviceName,
        decimal laborPrice,
        decimal partsTotal,
        decimal suppliesTotal)
    {
        if (serviceId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderServiceId);

        if (string.IsNullOrWhiteSpace(serviceName))
            throw new DomainException(DomainErrorMessages.InvalidServiceName);

        if (laborPrice < QuoteConstants.MinItemValue)
            throw new DomainException(DomainErrorMessages.QuoteInvalidLaborPrice);

        if (partsTotal < QuoteConstants.MinItemValue)
            throw new DomainException(DomainErrorMessages.QuoteInvalidPartsTotal);

        if (suppliesTotal < QuoteConstants.MinItemValue)
            throw new DomainException(DomainErrorMessages.QuoteInvalidSuppliesTotal);

        return new QuoteItem
        {
            Id = Guid.NewGuid(),
            ServiceId = serviceId,
            ServiceName = serviceName,
            LaborPrice = laborPrice,
            PartsTotal = partsTotal,
            SuppliesTotal = suppliesTotal,
            Subtotal = laborPrice + partsTotal + suppliesTotal
        };
    }
}
