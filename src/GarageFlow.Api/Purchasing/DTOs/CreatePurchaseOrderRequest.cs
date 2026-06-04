namespace GarageFlow.Api.Purchasing.DTOs;

public sealed record CreatePurchaseOrderRequest(
    IReadOnlyList<Guid>? SeparationOrderIds,
    IReadOnlyList<CreatePurchaseItemRequest>? Items);
