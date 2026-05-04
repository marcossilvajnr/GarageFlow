namespace GarageFlow.Api.DTOs.Stock;

public sealed record PagedSeparationOrderResponse(
    IReadOnlyList<SeparationOrderResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
