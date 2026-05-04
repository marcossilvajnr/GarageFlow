namespace GarageFlow.Application.ServiceOrders.DTOs;

public sealed record PagedServiceOrderResult(
    IReadOnlyList<ServiceOrderDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
