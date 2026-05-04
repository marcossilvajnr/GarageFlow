using GarageFlow.Application.Services.Commands;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Services.Handlers;

public sealed class RemoveServiceSupplyHandler(IServiceRepository serviceRepository)
{
    public async Task HandleAsync(RemoveServiceSupplyCommand command, CancellationToken cancellationToken = default)
    {
        var service = await serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);
        if (service is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceNotFound(command.ServiceId));

        service.RemoveSupply(command.SupplyId);

        await serviceRepository.SaveChangesAsync(cancellationToken);
    }
}
