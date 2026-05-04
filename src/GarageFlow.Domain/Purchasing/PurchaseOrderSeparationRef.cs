namespace GarageFlow.Domain.Purchasing;

/// <summary>
/// Wrapper for a SeparationOrderId reference stored in the purchase order.
/// Used as an owned EF Core navigation entity.
/// </summary>
public sealed class PurchaseOrderSeparationRef
{
    public Guid SeparationOrderId { get; private set; }

    private PurchaseOrderSeparationRef() { }

    internal static PurchaseOrderSeparationRef From(Guid separationOrderId) =>
        new() { SeparationOrderId = separationOrderId };
}
