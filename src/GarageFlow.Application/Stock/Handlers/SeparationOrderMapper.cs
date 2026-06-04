using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Mappers;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

internal static class SeparationOrderMapper
{
    internal static SeparationOrderDto ToDto(SeparationOrder separationOrder) =>
        new(
            separationOrder.Id,
            separationOrder.ExecutionOrderId,
            SeparationOrderStatusMapper.ToApplication(separationOrder.Status),
            separationOrder.Parts.Select(ToPartItemDto).ToList(),
            separationOrder.Supplies.Select(ToSupplyItemDto).ToList(),
            separationOrder.StockistId,
            separationOrder.ConfirmedByStockistAt,
            separationOrder.ConfirmedByMechanicAt,
            separationOrder.CreatedAt);

    private static SeparationPartItemDto ToPartItemDto(SeparationPartItem item) =>
        new(item.PartId, item.PartName, item.Quantity, item.IsReserved);

    private static SeparationSupplyItemDto ToSupplyItemDto(SeparationSupplyItem item) =>
        new(item.SupplyId, item.SupplyName, item.Quantity, SupplyUnitMapper.ToApplication(item.Unit), item.IsReserved);
}
