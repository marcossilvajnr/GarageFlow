using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record ServiceOrderServiceHistoryDto(
    Guid Id,
    Guid ServiceId,
    ServiceOrderServiceAction Action,
    ServiceSource Source,
    Guid ActorId,
    DateTime OccurredAt,
    string? Reason);
