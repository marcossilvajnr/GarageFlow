namespace GarageFlow.Application.ServiceOrders.Commands;

public sealed record AddServiceToServiceOrderCommand(
    Guid ServiceOrderId,
    Guid ServiceId,
    Guid ActorId);
