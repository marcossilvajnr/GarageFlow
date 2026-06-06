using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Application.Customers.Queries;
using GarageFlow.Domain.Customers;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Customers.Handlers;

public sealed class GetCustomerByIdHandler(ICustomerRepository repository)
{
    public async Task<CustomerDto> HandleAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetByIdAsync(query.Id, cancellationToken)
            ?? throw new EntityNotFoundException(DomainErrorMessages.CustomerNotFound(query.Id));

        return CustomerMapper.ToDto(customer);
    }
}
