using GarageFlow.Application.Parts.Commands;
using GarageFlow.Application.Parts.DTOs;
using GarageFlow.Domain.Parts;

namespace GarageFlow.Application.Parts.Handlers;

public sealed class CreatePartHandler(IPartRepository repository)
{
    public async Task<PartDto> HandleAsync(CreatePartCommand command, CancellationToken cancellationToken = default)
    {
        var part = Part.Create(
            command.Name,
            command.Code,
            command.Sku,
            command.UnitOfMeasure,
            command.UnitPrice);

        await repository.AddAsync(part, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return PartMapper.ToDto(part);
    }
}
