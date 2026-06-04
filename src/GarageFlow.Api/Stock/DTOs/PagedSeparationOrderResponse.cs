namespace GarageFlow.Api.Stock.DTOs;

public sealed record PagedSeparationOrderResponse(
    IReadOnlyList<SeparationOrderResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
