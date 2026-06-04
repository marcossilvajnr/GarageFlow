using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Mappers;
using GarageFlow.Domain.Stock;
using DomainStock = GarageFlow.Domain.Stock.Stock;

namespace GarageFlow.Application.Stock.Handlers;

internal static class StockMapper
{
    public static StockPositionDto ToPositionDto(DomainStock stock) =>
        new(
            stock.Id,
            stock.ItemId,
            StockItemTypeMapper.ToApplication(stock.ItemType),
            stock.TotalQuantity,
            stock.ReservedQuantity,
            stock.AvailableQuantity,
            stock.MinimumQuantity,
            stock.CreatedAt,
            stock.UpdatedAt);

    public static StockOperationDto ToOperationDto(StockOperation operation) =>
        new(
            operation.Id,
            StockOperationTypeMapper.ToApplication(operation.Type),
            operation.Quantity,
            operation.Reason,
            operation.ReferenceId,
            operation.ReferenceType,
            operation.PerformedBy,
            operation.CreatedAt);
}
