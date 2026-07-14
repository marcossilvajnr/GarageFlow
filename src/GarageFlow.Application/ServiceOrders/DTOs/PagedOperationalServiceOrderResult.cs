namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record PagedOperationalServiceOrderResult(
    IReadOnlyList<OperationalServiceOrderDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
