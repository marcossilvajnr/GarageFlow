using GarageFlow.Application.Purchasing.Commands;
using GarageFlow.Application.Purchasing.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Purchasing;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Purchasing.Handlers;

public sealed class CompletePurchaseOrderHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    ISeparationOrderRepository separationOrderRepository,
    IStockRepository stockRepository)
{
    public async Task<PurchaseOrderDto> HandleAsync(
        CompletePurchaseOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(command.Id, cancellationToken);
        if (purchaseOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.PurchaseOrderNotFound(command.Id));

        purchaseOrder.Complete();

        foreach (var separationOrderId in purchaseOrder.SeparationOrderIds)
        {
            var separationOrder = await separationOrderRepository.GetByIdAsync(separationOrderId, cancellationToken);
            if (separationOrder is null)
                throw new EntityNotFoundException(DomainErrorMessages.SeparationOrderNotFound(separationOrderId));

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
        }

        await purchaseOrderRepository.SaveChangesAsync(cancellationToken);

        return PurchaseOrderMapper.ToDto(purchaseOrder);
    }
}
