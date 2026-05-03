using GarageFlow.Application.Suppliers.Commands;
using GarageFlow.Application.Suppliers.DTOs;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.Suppliers;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Application.Suppliers.Handlers;

public sealed class UpdateSupplierHandler(ISupplierRepository repository)
{
    public async Task<SupplierDto> HandleAsync(UpdateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        var supplier = await repository.GetByIdAsync(command.Id, cancellationToken);
        if (supplier is null)
            throw new EntityNotFoundException(DomainErrorMessages.SupplierNotFound(command.Id));

        var address = Address.Create(
            command.Street, command.Number, command.Complement,
            command.Neighborhood, command.City, command.State, command.ZipCode);

        supplier.Update(command.Name, command.Email, command.PhoneNumber, address);

        repository.Update(supplier);
        await repository.SaveChangesAsync(cancellationToken);

        return SupplierMapper.ToDto(supplier);
    }
}
