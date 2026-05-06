using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class DeliverServiceOrderHandler(IServiceOrderRepository serviceOrderRepository)
{
    public async Task<ServiceOrderDto> HandleAsync(
        DeliverServiceOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(command.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(command.ServiceOrderId));

        serviceOrder.Deliver();

        await serviceOrderRepository.SaveChangesAsync(cancellationToken);

        return ServiceOrderMapper.ToDto(serviceOrder);
    }
}
