using GarageFlow.Application.Purchasing.Enums;

namespace GarageFlow.Api.Purchasing.DTOs;

public sealed record PurchaseOrderResponse(
    Guid Id,
    IReadOnlyList<Guid> SeparationOrderIds,
    Guid? SupplierId,
    Guid? EmployeeId,
    PurchaseOrderStatus Status,
    IReadOnlyList<PurchaseItemResponse> Items,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt);
