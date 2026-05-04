using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Domain.ServiceOrders;

namespace GarageFlow.Application.ServiceOrders.Handlers;

internal static class ServiceOrderMapper
{
    internal static ServiceOrderDto ToDto(ServiceOrder serviceOrder) =>
        new(
            serviceOrder.Id,
            serviceOrder.CustomerId,
            serviceOrder.VehicleId,
            serviceOrder.Status,
            serviceOrder.CreatedAt,
            serviceOrder.UpdatedAt,
            serviceOrder.Services.Select(ToServiceItemDto).ToList(),
            serviceOrder.ServiceHistory.Select(ToServiceHistoryDto).ToList());

    private static ServiceOrderServiceItemDto ToServiceItemDto(ServiceOrderServiceItem item) =>
        new(
            item.Id,
            item.ServiceId,
            item.Source,
            item.AddedByActorId,
            item.AddedAt,
            item.IsActive,
            item.RemovedAt,
            item.RemovedByActorId,
            item.RemovalReason);

    private static ServiceOrderServiceHistoryDto ToServiceHistoryDto(ServiceOrderServiceHistory history) =>
        new(
            history.Id,
            history.ServiceId,
            history.Action,
            history.Source,
            history.ActorId,
            history.OccurredAt,
            history.Reason);
}
