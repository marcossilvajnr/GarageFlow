namespace GarageFlow.Domain.ServiceOrders;

public sealed class ServiceOrderServiceItem
{
    public Guid Id { get; private set; }
    public Guid ServiceId { get; private set; }
    public ServiceSource Source { get; private set; }
    public Guid AddedByActorId { get; private set; }
    public DateTime AddedAt { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? RemovedAt { get; private set; }
    public Guid? RemovedByActorId { get; private set; }
    public string? RemovalReason { get; private set; }

    private ServiceOrderServiceItem() { }

    internal static ServiceOrderServiceItem Create(Guid serviceId, Guid actorId, ServiceSource source)
        => new()
        {
            Id = Guid.NewGuid(),
            ServiceId = serviceId,
            Source = source,
            AddedByActorId = actorId,
            AddedAt = DateTime.UtcNow,
            IsActive = true
        };

    internal void Remove(Guid actorId, string reason, DateTime occurredAt)
    {
        IsActive = false;
        RemovedByActorId = actorId;
        RemovalReason = reason;
        RemovedAt = occurredAt;
    }
}
