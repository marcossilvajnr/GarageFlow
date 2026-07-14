using GarageFlow.Application.ServiceOrders.Enums;

namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record OperationalServiceOrderDto(
    Guid ServiceOrderId,
    ServiceOrderStatus Status,
    string Label);
