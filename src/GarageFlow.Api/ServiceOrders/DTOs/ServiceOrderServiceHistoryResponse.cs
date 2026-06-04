using GarageFlow.Application.ServiceOrders.Enums;

namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record ServiceOrderServiceHistoryResponse(
    Guid Id,
    Guid ServiceId,
    ServiceOrderServiceAction Action,
    ServiceSource Source,
    Guid ActorId,
    DateTime OccurredAt,
    string? Reason);
