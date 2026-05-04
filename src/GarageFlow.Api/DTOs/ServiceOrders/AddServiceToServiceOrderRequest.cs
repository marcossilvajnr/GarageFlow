namespace GarageFlow.Api.DTOs.ServiceOrders;

public sealed record AddServiceToServiceOrderRequest(
    Guid ServiceId,
    Guid ActorId);
