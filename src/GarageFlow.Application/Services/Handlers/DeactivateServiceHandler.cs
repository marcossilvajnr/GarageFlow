using GarageFlow.Application.Services.Commands;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Services.Handlers;

public sealed class DeactivateServiceHandler(IServiceRepository repository)
{
    public async Task HandleAsync(DeactivateServiceCommand command, CancellationToken cancellationToken = default)
    {
        var service = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (service is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceNotFound(command.Id));

        service.Deactivate();

        repository.Update(service);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
