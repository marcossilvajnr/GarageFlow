using GarageFlow.Domain.Purchasing;

namespace GarageFlow.Api.DTOs.Purchasing;

public sealed record PurchaseOrderResponse(
    Guid Id,
    IReadOnlyList<Guid> SeparationOrderIds,
    Guid? SupplierId,
    PurchaseOrderStatus Status,
    IReadOnlyList<PurchaseItemResponse> Items,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt);
