using GarageFlow.Application.Supplies.DTOs;
using GarageFlow.Application.Supplies.Queries;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Supplies.Handlers;

public sealed class GetSupplyByIdHandler(ISupplyRepository repository)
{
    public async Task<SupplyDto> HandleAsync(GetSupplyByIdQuery query, CancellationToken cancellationToken = default)
    {
        var supply = await repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrorMessages.SupplyNotFound(query.Id));

        return SupplyMapper.ToDto(supply);
    }
}
