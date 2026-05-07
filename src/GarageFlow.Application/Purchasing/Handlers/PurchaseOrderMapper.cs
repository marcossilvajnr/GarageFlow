using GarageFlow.Application.Purchasing.DTOs;
using GarageFlow.Domain.Purchasing;

namespace GarageFlow.Application.Purchasing.Handlers;

internal static class PurchaseOrderMapper
{
    internal static PurchaseOrderDto ToDto(PurchaseOrder purchaseOrder) =>
        new(
            purchaseOrder.Id,
            purchaseOrder.SeparationOrderIds.ToList(),
            purchaseOrder.SupplierId,
            purchaseOrder.EmployeeId,
            purchaseOrder.Status,
            purchaseOrder.Items.Select(ToItemDto).ToList(),
            purchaseOrder.CreatedAt,
            purchaseOrder.StartedAt,
            purchaseOrder.CompletedAt);

    private static PurchaseItemDto ToItemDto(PurchaseItem item) =>
        new(item.ItemId, item.ItemType, item.ItemName, item.Quantity, item.UnitPrice, item.Subtotal);
}
