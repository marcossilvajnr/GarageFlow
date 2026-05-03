using GarageFlow.Application.Parts.Commands;
using GarageFlow.Application.Parts.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Parts.Handlers;

public sealed class UpdatePartHandler(IPartRepository repository)
{
    public async Task<PartDto> HandleAsync(UpdatePartCommand command, CancellationToken cancellationToken = default)
    {
        var part = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (part is null)
            throw new EntityNotFoundException(DomainErrorMessages.PartNotFound(command.Id));

        part.Update(command.Name, command.UnitOfMeasure, command.UnitPrice);

        repository.Update(part);
        await repository.SaveChangesAsync(cancellationToken);

        return PartMapper.ToDto(part);
    }
}
