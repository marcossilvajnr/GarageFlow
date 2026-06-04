namespace GarageFlow.Api.Supplies.DTOs;

public sealed record PagedSupplyResponse(
    IReadOnlyList<SupplyResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
