using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class ReserveStockHandler(
    IStockRepository stockRepository,
    IPartRepository partRepository,
    ISupplyRepository supplyRepository)
{
    public async Task<StockPositionDto> HandleAsync(ReserveStockCommand command, CancellationToken cancellationToken = default)
    {
        await StockItemExistenceValidator.EnsureExistsAsync(
            command.ItemId,
            command.ItemType,
            partRepository,
            supplyRepository,
            cancellationToken);

        var stock = await stockRepository.GetByItemAsync(command.ItemId, command.ItemType, cancellationToken);
        if (stock is null)
            throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(command.ItemType, command.ItemId));

        stock.Reserve(command.Quantity, command.Reason, command.ReferenceId);

        await stockRepository.SaveChangesAsync(cancellationToken);
        return StockMapper.ToPositionDto(stock);
    }
}
