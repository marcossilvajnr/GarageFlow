using GarageFlow.Application.ServiceOrders.Enums;

namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record ServiceOrderStatusDto(
    Guid ServiceOrderId,
    ServiceOrderStatus Status,
    string Label,
    DateTime? UpdatedAt);
