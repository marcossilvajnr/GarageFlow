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
            serviceOrder.UpdatedAt);
}
