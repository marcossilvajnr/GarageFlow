using GarageFlow.Application.Customers.Commands;
using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Application.Customers.Handlers;

public sealed class CreateCustomerHandler(ICustomerRepository repository)
{
    public async Task<CustomerDto> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var address = Address.Create(
            command.Street, command.Number, command.Complement,
            command.Neighborhood, command.City, command.State, command.ZipCode);

        var customer = Customer.Create(
            command.Name,
            command.DocumentType,
            command.Document,
            command.Email,
            command.PhoneNumber,
            address);

        await repository.AddAsync(customer, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return CustomerMapper.ToDto(customer);
    }
}
