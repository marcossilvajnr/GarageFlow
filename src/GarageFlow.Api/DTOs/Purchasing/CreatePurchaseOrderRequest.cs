namespace GarageFlow.Api.DTOs.Purchasing;

public sealed record CreatePurchaseOrderRequest(
    IReadOnlyList<Guid>? SeparationOrderIds,
    IReadOnlyList<CreatePurchaseItemRequest>? Items);
