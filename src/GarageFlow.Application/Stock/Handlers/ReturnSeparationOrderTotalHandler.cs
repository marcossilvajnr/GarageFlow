using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class ReturnSeparationOrderTotalHandler(
    ISeparationOrderRepository separationOrderRepository,
    IStockRepository stockRepository)
{
    public async Task<SeparationOrderDto> HandleAsync(ReturnSeparationOrderTotalCommand command, CancellationToken cancellationToken = default)
    {
        var separationOrder = await separationOrderRepository.GetByIdAsync(command.SeparationOrderId, cancellationToken);
        if (separationOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.SeparationOrderNotFound(command.SeparationOrderId));

        separationOrder.EnsureEligibleForTotalReturnBeforeMechanicReceipt();

        foreach (var part in separationOrder.Parts)
        {
            var stock = await stockRepository.GetByItemAsync(part.PartId, StockItemType.Part, cancellationToken);
            if (stock is null)
                throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(StockItemType.Part, part.PartId));

            stock.Entry(part.Quantity, referenceId: separationOrder.Id);
        }

        foreach (var supply in separationOrder.Supplies)
        {
            var stock = await stockRepository.GetByItemAsync(supply.SupplyId, StockItemType.Supply, cancellationToken);
            if (stock is null)
                throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(StockItemType.Supply, supply.SupplyId));

            stock.Entry(supply.Quantity, referenceId: separationOrder.Id);
        }

        separationOrder.ReturnTotalBeforeMechanicReceipt();

        await separationOrderRepository.SaveChangesAsync(cancellationToken);

        return SeparationOrderMapper.ToDto(separationOrder);
    }
}
