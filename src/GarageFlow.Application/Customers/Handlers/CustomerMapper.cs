using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Domain.Customers;

namespace GarageFlow.Application.Customers.Handlers;

internal static class CustomerMapper
{
    internal static CustomerDto ToDto(Customer customer) => new(
        customer.Id,
        customer.Name,
        customer.DocumentType,
        customer.DocumentType == Domain.Customers.CustomerDocumentType.Cpf
            ? customer.Cpf!.Value
            : customer.Cnpj!.Value,
        customer.Email.Value,
        customer.PhoneNumber.Value,
        new AddressDto(
            customer.Address.Street,
            customer.Address.Number,
            customer.Address.Complement,
            customer.Address.Neighborhood,
            customer.Address.City,
            customer.Address.State,
            customer.Address.ZipCode),
        customer.IsActive,
        customer.CreatedAt,
        customer.UpdatedAt);
}
