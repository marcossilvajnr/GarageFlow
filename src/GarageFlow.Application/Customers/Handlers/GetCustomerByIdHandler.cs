using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Application.Customers.Queries;
using GarageFlow.Domain.Customers;

namespace GarageFlow.Application.Customers.Handlers;

public sealed class GetCustomerByIdHandler(ICustomerRepository repository)
{
    public async Task<CustomerDto?> HandleAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default)
    {
        var customer = await repository.GetByIdAsync(query.Id, cancellationToken);
        return customer is null ? null : CustomerMapper.ToDto(customer);
    }
}
