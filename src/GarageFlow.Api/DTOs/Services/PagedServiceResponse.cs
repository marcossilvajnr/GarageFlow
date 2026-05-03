namespace GarageFlow.Api.DTOs.Services;

public sealed record PagedServiceResponse(
    IReadOnlyList<ServiceResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
