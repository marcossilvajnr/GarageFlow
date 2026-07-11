using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Application.ServiceOrders.Mappers;
using GarageFlow.Application.ServiceOrders.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class GetServiceOrderStatusHandler(IServiceOrderRepository serviceOrderRepository)
{
    public async Task<ServiceOrderStatusDto> HandleAsync(GetServiceOrderStatusQuery query, CancellationToken cancellationToken = default)
    {
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(query.Id, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(query.Id));

        var status = ServiceOrderStatusMapper.ToApplication(serviceOrder.Status);

        return new ServiceOrderStatusDto(
            serviceOrder.Id,
            status,
            ServiceOrderStatusLabelMapper.ToLabel(status),
            serviceOrder.UpdatedAt);
    }
}
