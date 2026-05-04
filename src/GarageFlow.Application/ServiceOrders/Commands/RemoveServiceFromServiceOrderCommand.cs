namespace GarageFlow.Application.ServiceOrders.Commands;

public sealed record RemoveServiceFromServiceOrderCommand(
    Guid ServiceOrderId,
    Guid ServiceId,
    Guid ActorId,
    string Reason);
