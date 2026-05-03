using GarageFlow.Application.Customers.DTOs;
using GarageFlow.Application.Customers.Queries;
using GarageFlow.Domain.Customers;

namespace GarageFlow.Application.Customers.Handlers;

public sealed class ListCustomersHandler(ICustomerRepository repository)
{
    public async Task<PagedResult<CustomerDto>> HandleAsync(ListCustomersQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? CustomersPaginationDefaults.DefaultPage : query.Page;
        var pageSize = query.PageSize < 1 ? CustomersPaginationDefaults.DefaultPageSize : query.PageSize;

        var (items, totalCount) = await repository.ListAsync(page, pageSize, cancellationToken);

        return new PagedResult<CustomerDto>(
            items.Select(CustomerMapper.ToDto).ToList(),
            page,
            pageSize,
            totalCount);
    }
}
