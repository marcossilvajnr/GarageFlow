using GarageFlow.Application.Purchasing.Commands;
using GarageFlow.Application.Purchasing.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Purchasing;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Purchasing.Handlers;

public sealed class CompletePurchaseOrderHandler(IPurchaseOrderRepository purchaseOrderRepository)
{
    public async Task<PurchaseOrderDto> HandleAsync(
        CompletePurchaseOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(command.Id, cancellationToken);
        if (purchaseOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.PurchaseOrderNotFound(command.Id));

        purchaseOrder.Complete();

        await purchaseOrderRepository.SaveChangesAsync(cancellationToken);

        return PurchaseOrderMapper.ToDto(purchaseOrder);
    }
}
