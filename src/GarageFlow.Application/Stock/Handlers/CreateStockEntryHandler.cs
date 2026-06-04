using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Application.Stock.Mappers;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Stock;
using GarageFlow.Domain.Supplies;
using DomainStock = GarageFlow.Domain.Stock.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class CreateStockEntryHandler(
    IStockRepository stockRepository,
    IPartRepository partRepository,
    ISupplyRepository supplyRepository)
{
    public async Task<StockPositionDto> HandleAsync(CreateStockEntryCommand command, CancellationToken cancellationToken = default)
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
        {
            stock = DomainStock.Create(command.ItemId, itemType, 0, command.MinimumQuantity);
            stock.Entry(command.Quantity, command.Reason, command.ReferenceId);
            await stockRepository.AddAsync(stock, cancellationToken);
        }
        else
        {
            stock.Entry(command.Quantity, command.Reason, command.ReferenceId);
        }

        await stockRepository.SaveChangesAsync(cancellationToken);
        return StockMapper.ToPositionDto(stock);
    }
}
