namespace GarageFlow.Application.ServiceOrders.Commands;

public sealed record CreateServiceOrderCommand(Guid CustomerId, Guid VehicleId, Guid FrontDeskEmployeeId);
