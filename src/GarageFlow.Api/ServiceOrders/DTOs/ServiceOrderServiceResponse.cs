using GarageFlow.Application.ServiceOrders.Enums;

namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record ServiceOrderServiceResponse(
    Guid Id,
    Guid ServiceId,
    ServiceSource Source,
    Guid AddedByActorId,
    DateTime AddedAt,
    bool IsActive,
    DateTime? RemovedAt,
    Guid? RemovedByActorId,
    string? RemovalReason);
