namespace GarageFlow.Api.Employees.DTOs;

public sealed record PagedEmployeeResponse(
    IReadOnlyList<EmployeeResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);