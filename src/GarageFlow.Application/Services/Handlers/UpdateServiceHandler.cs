using GarageFlow.Application.Services.Commands;
using GarageFlow.Application.Services.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Services;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Services.Handlers;

public sealed class UpdateServiceHandler(IServiceRepository repository)
{
    public async Task<ServiceDto> HandleAsync(UpdateServiceCommand command, CancellationToken cancellationToken = default)
    {
        var service = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (service is null)
            throw new EntityNotFoundException(DomainErrorMessages.ServiceNotFound(command.Id));

        var nameExists = await repository.ExistsByNameExcludingIdAsync(command.Name, command.Id, cancellationToken);
        if (nameExists)
            throw new DuplicateServiceDataException(DomainErrorMessages.DuplicateServiceName);

        service.Update(command.Name, command.Description, command.BasePrice, command.EstimatedDurationMinutes);

        repository.Update(service);
        await repository.SaveChangesAsync(cancellationToken);

        return ServiceMapper.ToDto(service);
    }
}
