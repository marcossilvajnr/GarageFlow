using GarageFlow.Application.Suppliers.Commands;
using GarageFlow.Application.Suppliers.DTOs;
using GarageFlow.Domain.Suppliers;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Application.Suppliers.Handlers;

public sealed class CreateSupplierHandler(ISupplierRepository repository)
{
    public async Task<SupplierDto> HandleAsync(CreateSupplierCommand command, CancellationToken cancellationToken = default)
    {
        var address = Address.Create(
            command.Street, command.Number, command.Complement,
            command.Neighborhood, command.City, command.State, command.ZipCode);

        var supplier = Supplier.Create(
            command.Name,
            command.Cnpj,
            command.Email,
            command.PhoneNumber,
            address);

        await repository.AddAsync(supplier, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return SupplierMapper.ToDto(supplier);
    }
}
