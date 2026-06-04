using GarageFlow.Application.Purchasing.Commands;
using GarageFlow.Application.Purchasing.DTOs;
using GarageFlow.Application.Purchasing.Mappers;
using GarageFlow.Domain.Purchasing;

namespace GarageFlow.Application.Purchasing.Handlers;

public sealed class CreatePurchaseOrderHandler(IPurchaseOrderRepository purchaseOrderRepository)
{
    public async Task<PurchaseOrderDto> HandleAsync(
        CreatePurchaseOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var items = command.Items
            .Select(i => PurchaseItem.Create(
                i.ItemId,
                PurchaseItemTypeMapper.ToDomain(i.ItemType),
                i.ItemName,
                i.Quantity,
                i.UnitPrice))
            .ToList();

        var purchaseOrder = PurchaseOrder.Create(command.SeparationOrderIds, items);

        await purchaseOrderRepository.AddAsync(purchaseOrder, cancellationToken);
        await purchaseOrderRepository.SaveChangesAsync(cancellationToken);

        return PurchaseOrderMapper.ToDto(purchaseOrder);
    }
}
