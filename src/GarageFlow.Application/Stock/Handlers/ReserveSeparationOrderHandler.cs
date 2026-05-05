using GarageFlow.Application.Stock.Commands;
using GarageFlow.Application.Stock.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Stock.Handlers;

public sealed class ReserveSeparationOrderHandler(
    ISeparationOrderRepository separationOrderRepository,
    IStockRepository stockRepository)
{
    public async Task<SeparationOrderDto> HandleAsync(ReserveSeparationOrderCommand command, CancellationToken cancellationToken = default)
    {
        var separationOrder = await separationOrderRepository.GetByIdAsync(command.SeparationOrderId, cancellationToken);
        if (separationOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.SeparationOrderNotFound(command.SeparationOrderId));

        if (separationOrder.Status != SeparationOrderStatus.Pending)
            throw new InvalidSeparationOrderStatusTransitionException(DomainErrorMessages.SeparationOrderNotPending);

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

        separationOrder.Reserve();

        await separationOrderRepository.SaveChangesAsync(cancellationToken);

        return SeparationOrderMapper.ToDto(separationOrder);
    }
}
