namespace GarageFlow.Api.DTOs.ServiceOrders;

public sealed record CreateServiceOrderRequest(Guid CustomerId, Guid VehicleId);
