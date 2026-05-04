using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Domain.Stock;

public sealed class SeparationSupplyItem
{
    public Guid SupplyId { get; private set; }
    public string SupplyName { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public SupplyUnit Unit { get; private set; }
    public bool IsReserved { get; private set; }

    private SeparationSupplyItem() { }

    public static SeparationSupplyItem Create(Guid supplyId, string supplyName, decimal quantity, SupplyUnit unit)
    {
        if (supplyId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidSeparationSupplyId);

        if (string.IsNullOrWhiteSpace(supplyName))
            throw new DomainException(DomainErrorMessages.InvalidSeparationItemName);

        if (quantity <= 0)
            throw new DomainException(DomainErrorMessages.InvalidSeparationItemQuantity);

        return new SeparationSupplyItem
        {
            SupplyId = supplyId,
            SupplyName = supplyName.Trim(),
            Quantity = quantity,
            Unit = unit,
            IsReserved = false
        };
    }

    internal void MarkReserved()
    {
        IsReserved = true;
    }
}
