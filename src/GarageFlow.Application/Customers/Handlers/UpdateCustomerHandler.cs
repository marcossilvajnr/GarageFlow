using GarageFlow.Application.Customers.Commands;
using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Application.Customers.Handlers;

public sealed class UpdateCustomerHandler(ICustomerRepository repository)
{
    public async Task<CustomerDto> HandleAsync(UpdateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrorMessages.CustomerNotFound(command.Id));

        var address = Address.Create(
            command.Street, command.Number, command.Complement,
            command.Neighborhood, command.City, command.State, command.ZipCode);

        customer.Update(command.Name, command.Email, command.PhoneNumber, address);

        repository.Update(customer);
        await repository.SaveChangesAsync(cancellationToken);

        return CustomerMapper.ToDto(customer);
    }
}
