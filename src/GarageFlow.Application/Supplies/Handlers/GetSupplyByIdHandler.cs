using GarageFlow.Application.Supplies.DTOs;
using GarageFlow.Application.Supplies.Queries;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Supplies.Handlers;

public sealed class GetSupplyByIdHandler(ISupplyRepository repository)
{
    public async Task<SupplyDto?> HandleAsync(GetSupplyByIdQuery query, CancellationToken cancellationToken = default)
    {
        var supply = await repository.GetByIdAsync(query.Id, cancellationToken);
        return supply is null ? null : SupplyMapper.ToDto(supply);
    }
}
