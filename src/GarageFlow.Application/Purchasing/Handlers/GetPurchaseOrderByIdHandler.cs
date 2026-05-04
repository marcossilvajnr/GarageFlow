using GarageFlow.Application.Purchasing.DTOs;
using GarageFlow.Application.Purchasing.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Purchasing;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Purchasing.Handlers;

public sealed class GetPurchaseOrderByIdHandler(IPurchaseOrderRepository purchaseOrderRepository)
{
    public async Task<PurchaseOrderDto> HandleAsync(
        GetPurchaseOrderByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var purchaseOrder = await purchaseOrderRepository.GetByIdAsync(query.Id, cancellationToken);
        if (purchaseOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.PurchaseOrderNotFound(query.Id));

        return PurchaseOrderMapper.ToDto(purchaseOrder);
    }
}
