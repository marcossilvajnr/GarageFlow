namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record AddServiceToServiceOrderRequest(
    Guid ServiceId,
    Guid ActorId);
