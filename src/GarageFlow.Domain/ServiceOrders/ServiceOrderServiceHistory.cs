namespace GarageFlow.Domain.ServiceOrders;

public sealed class ServiceOrderServiceHistory
{
    public Guid Id { get; private set; }
    public Guid ServiceId { get; private set; }
    public ServiceOrderServiceAction Action { get; private set; }
    public ServiceSource Source { get; private set; }
    public Guid ActorId { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public string? Reason { get; private set; }

    private ServiceOrderServiceHistory() { }

    internal static ServiceOrderServiceHistory Create(
        Guid serviceId,
        ServiceOrderServiceAction action,
        ServiceSource source,
        Guid actorId,
        DateTime occurredAt,
        string? reason = null)
        => new()
        {
            Id = Guid.NewGuid(),
            ServiceId = serviceId,
            Action = action,
            Source = source,
            ActorId = actorId,
            OccurredAt = occurredAt,
            Reason = reason
        };
}
