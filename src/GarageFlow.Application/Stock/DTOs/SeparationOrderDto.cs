using GarageFlow.Application.Stock.Enums;

namespace GarageFlow.Application.Stock.DTOs;

public sealed record SeparationOrderDto(
    Guid Id,
    Guid ExecutionOrderId,
    SeparationOrderStatus Status,
    IReadOnlyList<SeparationPartItemDto> Parts,
    IReadOnlyList<SeparationSupplyItemDto> Supplies,
    Guid? StockistId,
    DateTime? ConfirmedByStockistAt,
    DateTime? ConfirmedByMechanicAt,
    DateTime CreatedAt);
