using GarageFlow.Application.Parts.Commands;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Parts.Handlers;

public sealed class DeactivatePartHandler(IPartRepository repository)
{
    public async Task HandleAsync(DeactivatePartCommand command, CancellationToken cancellationToken = default)
    {
        var part = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (part is null)
            throw new EntityNotFoundException(DomainErrorMessages.PartNotFound(command.Id));

        part.Deactivate();

        repository.Update(part);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
