namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record CreateServiceOrderRequest(Guid CustomerId, Guid VehicleId, Guid FrontDeskEmployeeId);
