using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Domain.Stock;

public sealed class SeparationPartItem
{
    public Guid PartId { get; private set; }
    public string PartName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }
    public bool IsReserved { get; private set; }

    private SeparationPartItem() { }

    public static SeparationPartItem Create(Guid partId, string partName, int quantity)
    {
        if (partId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidSeparationPartId);

        if (string.IsNullOrWhiteSpace(partName))
            throw new DomainException(DomainErrorMessages.InvalidSeparationItemName);

        if (quantity <= 0)
            throw new DomainException(DomainErrorMessages.InvalidSeparationItemQuantity);

        return new SeparationPartItem
        {
            PartId = partId,
            PartName = partName.Trim(),
            Quantity = quantity,
            IsReserved = false
        };
    }

    internal void MarkReserved()
    {
        IsReserved = true;
    }

    internal void UnmarkReserved()
    {
        IsReserved = false;
    }
}
