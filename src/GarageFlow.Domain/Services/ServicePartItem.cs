namespace GarageFlow.Domain.Services;

public sealed class ServicePartItem
{
    public Guid PartId { get; private set; }
    public string PartName { get; private set; } = string.Empty;
    public int Quantity { get; private set; }

    private ServicePartItem() { }

    internal static ServicePartItem Create(Guid partId, string partName, int quantity)
        => new()
        {
            PartId = partId,
            PartName = partName,
            Quantity = quantity
        };
}
