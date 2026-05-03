using GarageFlow.Application.Supplies.Commands;
using GarageFlow.Application.Supplies.DTOs;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Supplies.Handlers;

public sealed class CreateSupplyHandler(ISupplyRepository repository)
{
    public async Task<SupplyDto> HandleAsync(CreateSupplyCommand command, CancellationToken cancellationToken = default)
    {
        var supply = Supply.Create(
            command.Name,
            command.Code,
            command.UnitOfMeasure,
            command.BaseCost,
            command.PreferredSupplierId);

        await repository.AddAsync(supply, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return SupplyMapper.ToDto(supply);
    }
}
