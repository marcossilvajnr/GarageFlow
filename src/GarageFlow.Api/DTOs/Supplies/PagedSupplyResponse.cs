namespace GarageFlow.Api.DTOs.Supplies;

public sealed record PagedSupplyResponse(
    IReadOnlyList<SupplyResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
