using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class ResumeSeparationOrderAfterPurchaseHandler(
    ISeparationOrderRepository separationOrderRepository,
    IStockRepository stockRepository)
{
    public async Task<SeparationOrderDto> HandleAsync(ResumeSeparationOrderAfterPurchaseCommand command, CancellationToken cancellationToken = default)
    {
        var separationOrder = await separationOrderRepository.GetByIdAsync(command.SeparationOrderId, cancellationToken);
        if (separationOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.SeparationOrderNotFound(command.SeparationOrderId));

        if (separationOrder.Status != SeparationOrderStatus.WaitingPurchase)
            throw new InvalidSeparationOrderStatusTransitionException(DomainErrorMessages.SeparationOrderNotWaitingPurchase);

        foreach (var part in separationOrder.Parts)
        {
            var stock = await stockRepository.GetByItemAsync(part.PartId, StockItemType.Part, cancellationToken);
            if (stock is null)
                throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(StockItemType.Part, part.PartId));

            stock.Reserve(part.Quantity, referenceId: separationOrder.Id);
        }

        foreach (var supply in separationOrder.Supplies)
        {
            var stock = await stockRepository.GetByItemAsync(supply.SupplyId, StockItemType.Supply, cancellationToken);
            if (stock is null)
                throw new EntityNotFoundException(DomainErrorMessages.StockNotFound(StockItemType.Supply, supply.SupplyId));

            stock.Reserve(supply.Quantity, referenceId: separationOrder.Id);
        }

        separationOrder.ResumeAfterPurchase();

        await separationOrderRepository.SaveChangesAsync(cancellationToken);

        return SeparationOrderMapper.ToDto(separationOrder);
    }
}
