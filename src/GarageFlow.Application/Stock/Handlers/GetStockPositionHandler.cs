using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class GetStockPositionHandler(IStockRepository repository)
{
    public async Task<StockPositionDto> HandleAsync(GetStockPositionQuery query, CancellationToken cancellationToken = default)
    {
        var stock = await repository.GetByItemAsync(query.ItemId, query.ItemType, cancellationToken);
        if (stock is null)
            throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(query.ItemType, query.ItemId));

        return StockMapper.ToPositionDto(stock);
    }
}
