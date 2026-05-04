using GarageFlow.Application.ServiceOrders.Commands;
using GarageFlow.Application.ServiceOrders.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.ServiceOrders;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.ServiceOrders.Handlers;

public sealed class StartDiagnosticHandler(IServiceOrderRepository serviceOrderRepository)
{
    public async Task<ServiceOrderDto> HandleAsync(
        StartDiagnosticCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.MechanicId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidDiagnosticMechanicId);

        var serviceOrder = await serviceOrderRepository.GetByIdAsync(command.ServiceOrderId, cancellationToken);
        if (serviceOrder is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceOrderNotFound(command.ServiceOrderId));

        serviceOrder.StartDiagnostic(command.MechanicId);

        await serviceOrderRepository.SaveChangesAsync(cancellationToken);

        return ServiceOrderMapper.ToDto(serviceOrder);
    }
}
