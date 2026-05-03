using GarageFlow.Application.Supplies.Commands;
using GarageFlow.Application.Supplies.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Supplies.Handlers;

public sealed class UpdateSupplyHandler(ISupplyRepository repository)
{
    public async Task<SupplyDto> HandleAsync(UpdateSupplyCommand command, CancellationToken cancellationToken = default)
    {
        var supply = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (supply is null)
            throw new EntityNotFoundException(DomainErrorMessages.SupplyNotFound(command.Id));

        supply.Update(command.Name, command.UnitOfMeasure, command.BaseCost, command.PreferredSupplierId);

        repository.Update(supply);
        await repository.SaveChangesAsync(cancellationToken);

        return SupplyMapper.ToDto(supply);
    }
}
