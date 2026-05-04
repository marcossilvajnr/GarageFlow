namespace GarageFlow.Application.Stock.DTOs;

public sealed record PagedSeparationOrderResult(
    IReadOnlyList<SeparationOrderDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
