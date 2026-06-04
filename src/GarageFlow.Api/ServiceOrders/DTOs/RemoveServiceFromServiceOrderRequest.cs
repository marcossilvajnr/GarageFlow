namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record RemoveServiceFromServiceOrderRequest(
    Guid ActorId,
    string Reason);
