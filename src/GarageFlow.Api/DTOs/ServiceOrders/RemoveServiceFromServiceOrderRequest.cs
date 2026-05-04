namespace GarageFlow.Api.DTOs.ServiceOrders;

public sealed record RemoveServiceFromServiceOrderRequest(
    Guid ActorId,
    string Reason);
