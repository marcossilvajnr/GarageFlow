using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class AddServiceToServiceOrderHandler(
    IServiceOrderRepository serviceOrderRepository,
    IServiceRepository serviceRepository)
{
    public async Task<ServiceOrderDto> HandleAsync(
        AddServiceToServiceOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ActorId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderActorId);

        var serviceOrder = await serviceOrderRepository.GetByIdAsync(command.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(command.ServiceOrderId));

        var service = await serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);
        if (service is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceNotFound(command.ServiceId));

        if (!service.IsActive)
            throw new DomainException(DomainErrorMessages.ServiceOrderServiceInactive);

        serviceOrder.AddService(command.ServiceId, command.ActorId, ServiceSource.FrontDesk);

        await serviceOrderRepository.SaveChangesAsync(cancellationToken);

        return ServiceOrderMapper.ToDto(serviceOrder);
    }
}
