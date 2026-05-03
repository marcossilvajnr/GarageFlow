using GarageFlow.Application.Parts.DTOs;
using GarageFlow.Application.Parts.Queries;
using GarageFlow.Domain.Parts;

namespace GarageFlow.Application.Parts.Handlers;

public sealed class GetPartByIdHandler(IPartRepository repository)
{
    public async Task<PartDto?> HandleAsync(GetPartByIdQuery query, CancellationToken cancellationToken = default)
    {
        var part = await repository.GetByIdAsync(query.Id, cancellationToken);
        return part is null ? null : PartMapper.ToDto(part);
    }
}
