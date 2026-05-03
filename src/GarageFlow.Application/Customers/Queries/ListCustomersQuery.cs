namespace GarageFlow.Application.Customers.Queries;

public sealed record ListCustomersQuery(
    int Page = CustomersPaginationDefaults.DefaultPage,
    int PageSize = CustomersPaginationDefaults.DefaultPageSize);
