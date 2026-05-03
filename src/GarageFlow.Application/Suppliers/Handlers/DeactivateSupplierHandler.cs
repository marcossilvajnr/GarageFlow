using GarageFlow.Application.Suppliers.Commands;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Suppliers;

namespace GarageFlow.Application.Suppliers.Handlers;

public sealed class DeactivateSupplierHandler(ISupplierRepository repository)
{
    public async Task HandleAsync(DeactivateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        var supplier = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (supplier is null)
            throw new EntityNotFoundException(DomainErrorMessages.SupplierNotFound(command.Id));

        supplier.Deactivate();

        repository.Update(supplier);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
