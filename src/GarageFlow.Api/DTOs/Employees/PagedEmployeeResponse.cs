namespace GarageFlow.Api.DTOs.Employees;

public sealed record PagedEmployeeResponse(
    IReadOnlyList<EmployeeResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);