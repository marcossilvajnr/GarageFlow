using GarageFlow.Application.ServiceOrders.Enums;

namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record OperationalServiceOrderResponse(
    Guid ServiceOrderId,
    ServiceOrderStatus Status,
    string Label);
