using GarageFlow.Application.Purchasing.Commands;
using GarageFlow.Application.Purchasing.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Purchasing;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Stock;

namespace GarageFlow.Application.Purchasing.Handlers;

public sealed class CompletePurchaseOrderHandler(
    IPurchaseOrderRepository purchaseOrderRepository,
    ISeparationOrderRepository separationOrderRepository)
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

            separationOrder.ResumeAfterPurchase();
        }

        await purchaseOrderRepository.SaveChangesAsync(cancellationToken);

        return PurchaseOrderMapper.ToDto(purchaseOrder);
    }
}
