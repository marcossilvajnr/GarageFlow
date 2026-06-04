namespace GarageFlow.Api.Stock.DTOs;

public sealed record CreateSeparationOrderRequest(
    Guid ExecutionOrderId,
    IReadOnlyList<CreateSeparationPartItemRequest>? Parts,
    IReadOnlyList<CreateSeparationSupplyItemRequest>? Supplies);
