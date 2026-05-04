using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Application.ServiceOrders.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class GetServiceOrderByIdHandler(IServiceOrderRepository serviceOrderRepository)
{
    public async Task<ServiceOrderDto> HandleAsync(GetServiceOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(query.Id, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(query.Id));

        return ServiceOrderMapper.ToDto(serviceOrder);
    }
}
