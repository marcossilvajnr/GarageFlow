using GarageFlow.Application.Supplies.Commands;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Supplies;

namespace GarageFlow.Application.Supplies.Handlers;

public sealed class DeactivateSupplyHandler(ISupplyRepository repository)
{
    public async Task HandleAsync(DeactivateSupplyCommand command, CancellationToken cancellationToken = default)
    {
        var supply = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (supply is null)
            throw new EntityNotFoundException(DomainErrorMessages.SupplyNotFound(command.Id));

        supply.Deactivate();

        repository.Update(supply);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
