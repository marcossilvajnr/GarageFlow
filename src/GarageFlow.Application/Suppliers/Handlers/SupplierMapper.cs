using GarageFlow.Application.Suppliers.DTOs;
using GarageFlow.Domain.Suppliers;

namespace GarageFlow.Application.Suppliers.Handlers;

public static class SupplierMapper
{
    public static SupplierDto ToDto(Supplier supplier) => new(
        supplier.Id,
        supplier.Name,
        supplier.Cnpj.Value,
        supplier.Email.Value,
        supplier.PhoneNumber.Value,
        new AddressDto(
            supplier.Address.Street,
            supplier.Address.Number,
            supplier.Address.Complement,
            supplier.Address.Neighborhood,
            supplier.Address.City,
            supplier.Address.State,
            supplier.Address.ZipCode),
        supplier.IsActive,
        supplier.CreatedAt,
        supplier.UpdatedAt);
}
