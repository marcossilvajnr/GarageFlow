using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class RemoveServiceFromServiceOrderHandler(
    IServiceOrderRepository serviceOrderRepository)
{
    public async Task HandleAsync(
        RemoveServiceFromServiceOrderCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.ActorId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidServiceOrderActorId);

        if (string.IsNullOrWhiteSpace(command.Reason))
            throw new DomainException(DomainErrorMessages.ServiceOrderServiceRemovalReasonRequired);

        var serviceOrder = await serviceOrderRepository.GetByIdAsync(command.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(command.ServiceOrderId));

        serviceOrder.RemoveService(command.ServiceId, command.ActorId, ServiceSource.FrontDesk, command.Reason);

        await serviceOrderRepository.SaveChangesAsync(cancellationToken);
    }
}
