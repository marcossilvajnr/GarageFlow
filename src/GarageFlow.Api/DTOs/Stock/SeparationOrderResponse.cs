using GarageFlow.Domain.Stock;

namespace GarageFlow.Api.DTOs.Stock;

public sealed record SeparationOrderResponse(
    Guid Id,
    Guid ExecutionOrderId,
    SeparationOrderStatus Status,
    IReadOnlyList<SeparationPartItemResponse> Parts,
    IReadOnlyList<SeparationSupplyItemResponse> Supplies,
    Guid? StockistId,
    DateTime? ConfirmedByStockistAt,
    DateTime? ConfirmedByMechanicAt,
    DateTime CreatedAt);
