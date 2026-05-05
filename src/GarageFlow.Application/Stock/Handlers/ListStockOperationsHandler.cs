using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class ListStockOperationsHandler(IStockRepository repository)
{
    public async Task<ListStockOperationsResult> HandleAsync(
        ListStockOperationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var stock = await repository.GetByItemAsync(query.ItemId, query.ItemType, cancellationToken);
        if (stock is null)
            throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(query.ItemType, query.ItemId));

        var (items, totalCount) = await repository.ListOperationsAsync(
            query.ItemId,
            query.ItemType,
            query.From,
            query.To,
            query.Page,
            query.PageSize,
            cancellationToken);

        return new ListStockOperationsResult(
            items.Select(StockMapper.ToOperationDto).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }
}
