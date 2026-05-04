using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class RemoveDiagnosticServiceHandler(IServiceOrderRepository serviceOrderRepository)
{
    public async Task HandleAsync(
        RemoveDiagnosticServiceCommand command,
        CancellationToken cancellationToken = default)
    {
        var serviceOrder = await serviceOrderRepository.GetByIdAsync(command.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(command.ServiceOrderId));

        serviceOrder.RemoveDiagnosticService(command.ServiceId);

        await serviceOrderRepository.SaveChangesAsync(cancellationToken);
    }
}
