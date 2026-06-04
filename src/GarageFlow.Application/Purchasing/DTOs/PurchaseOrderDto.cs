using GarageFlow.Application.Purchasing.Enums;

namespace GarageFlow.Application.Purchasing.DTOs;

public sealed record PurchaseOrderDto(
    Guid Id,
    IReadOnlyList<Guid> SeparationOrderIds,
    Guid? SupplierId,
    Guid? EmployeeId,
    PurchaseOrderStatus Status,
    IReadOnlyList<PurchaseItemDto> Items,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt);
