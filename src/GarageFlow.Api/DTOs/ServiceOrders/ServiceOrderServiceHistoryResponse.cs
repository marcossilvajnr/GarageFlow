using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Api.DTOs.ServiceOrders;

public sealed record ServiceOrderServiceHistoryResponse(
    Guid Id,
    Guid ServiceId,
    ServiceOrderServiceAction Action,
    ServiceSource Source,
    Guid ActorId,
    DateTime OccurredAt,
    string? Reason);
