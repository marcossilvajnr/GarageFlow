using GarageFlow.Application.Customers.Commands;
using GarageFlow.Domain.Customers;

namespace GarageFlow.Application.Customers.Handlers;

public sealed class DeactivateCustomerHandler(ICustomerRepository repository)
{
    public async Task HandleAsync(DeactivateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new KeyNotFoundException(CustomersErrorMessages.CustomerNotFound(command.Id));

        customer.Deactivate();

        repository.Update(customer);
        await repository.SaveChangesAsync(cancellationToken);
    }
}
