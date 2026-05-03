namespace GarageFlow.Api.DTOs.Customers;

public sealed record PagedCustomerResponse(
    IReadOnlyList<CustomerResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
