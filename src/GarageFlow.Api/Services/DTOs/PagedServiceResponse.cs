namespace GarageFlow.Api.Services.DTOs;

public sealed record PagedServiceResponse(
    IReadOnlyList<ServiceResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
