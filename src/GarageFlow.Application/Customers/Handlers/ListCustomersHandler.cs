using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Application.Customers.Queries;
using GarageFlow.Domain.Customers;

namespace GarageFlow.Application.Customers.Handlers;

public sealed class ListCustomersHandler(ICustomerRepository repository)
{
    public async Task<PagedResult<CustomerDto>> HandleAsync(ListCustomersQuery query, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await repository.ListAsync(query.Page, query.PageSize, cancellationToken);

        return new PagedResult<CustomerDto>(
            items.Select(CustomerMapper.ToDto).ToList(),
            query.Page,
            query.PageSize,
            totalCount);
    }
}
