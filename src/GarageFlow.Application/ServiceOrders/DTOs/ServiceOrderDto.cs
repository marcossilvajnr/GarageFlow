using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record ServiceOrderDto(
    Guid Id,
    Guid CustomerId,
    Guid VehicleId,
    ServiceOrderStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
