using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Api.DTOs.ServiceOrders;

public sealed record ServiceOrderResponse(
    Guid Id,
    Guid CustomerId,
    Guid VehicleId,
    ServiceOrderStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
