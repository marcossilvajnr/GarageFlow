using GarageFlow.Domain.Supplies;

namespace GarageFlow.Api.DTOs.Stock;

public sealed record CreateSeparationOrderRequest(
    Guid ExecutionOrderId,
    IReadOnlyList<CreateSeparationPartItemRequest>? Parts,
    IReadOnlyList<CreateSeparationSupplyItemRequest>? Supplies);
