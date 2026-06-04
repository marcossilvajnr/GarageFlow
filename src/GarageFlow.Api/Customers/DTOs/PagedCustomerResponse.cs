namespace GarageFlow.Api.Customers.DTOs;

public sealed record PagedCustomerResponse(
    IReadOnlyList<CustomerResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
