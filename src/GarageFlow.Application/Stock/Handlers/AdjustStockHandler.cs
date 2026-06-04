using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Mappers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class AdjustStockHandler(
    IStockRepository stockRepository,
    IPartRepository partRepository,
    ISupplyRepository supplyRepository)
{
    public async Task<StockPositionDto> HandleAsync(AdjustStockCommand command, CancellationToken cancellationToken = default)
    {
        var itemType = StockItemTypeMapper.ToDomain(command.ItemType);

        await StockItemExistenceValidator.EnsureExistsAsync(
            command.ItemId,
            itemType,
            partRepository,
            supplyRepository,
            cancellationToken);

        var stock = await stockRepository.GetByItemAsync(command.ItemId, itemType, cancellationToken);
        if (stock is null)
            throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(itemType, command.ItemId));

        stock.Adjust(command.QuantityDelta, command.Reason, command.ReferenceId);

        await stockRepository.SaveChangesAsync(cancellationToken);
        return StockMapper.ToPositionDto(stock);
    }
}
