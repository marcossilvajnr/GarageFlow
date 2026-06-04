using GarageFlow.Application.ServiceOrders.Enums;

namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record ServiceOrderServiceItemDto(
    Guid Id,
    Guid ServiceId,
    ServiceSource Source,
    Guid AddedByActorId,
    DateTime AddedAt,
    bool IsActive,
    DateTime? RemovedAt,
    Guid? RemovedByActorId,
    string? RemovalReason);
