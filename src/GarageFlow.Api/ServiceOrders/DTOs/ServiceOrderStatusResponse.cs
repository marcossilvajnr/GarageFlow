using GarageFlow.Application.ServiceOrders.Enums;

namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record ServiceOrderStatusResponse(
    Guid ServiceOrderId,
    ServiceOrderStatus Status,
    string Label,
    DateTime? UpdatedAt);
