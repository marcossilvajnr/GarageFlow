using GarageFlow.Domain.Supplies;

namespace GarageFlow.Domain.Services;

public sealed class ServiceSupplyItem
{
    public Guid SupplyId { get; private set; }
    public string SupplyName { get; private set; } = string.Empty;
    public decimal Quantity { get; private set; }
    public SupplyUnit Unit { get; private set; }

    private ServiceSupplyItem() { }

    internal static ServiceSupplyItem Create(Guid supplyId, string supplyName, decimal quantity, SupplyUnit unit)
        => new()
        {
            SupplyId = supplyId,
            SupplyName = supplyName,
            Quantity = quantity,
            Unit = unit
        };
}
