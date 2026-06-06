using GarageFlow.Application.Parts.DTOs;
using GarageFlow.Application.Parts.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Parts;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Parts.Handlers;

public sealed class GetPartByIdHandler(IPartRepository repository)
{
    public async Task<PartDto> HandleAsync(GetPartByIdQuery query, CancellationToken cancellationToken = default)
    {
        var part = await repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrorMessages.PartNotFound(query.Id));

        return PartMapper.ToDto(part);
    }
}
