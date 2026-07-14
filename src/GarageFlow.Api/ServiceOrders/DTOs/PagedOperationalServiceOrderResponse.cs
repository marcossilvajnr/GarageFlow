namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record PagedOperationalServiceOrderResponse(
    IReadOnlyList<OperationalServiceOrderResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
