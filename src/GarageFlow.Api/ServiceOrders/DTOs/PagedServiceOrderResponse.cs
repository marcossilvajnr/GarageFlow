namespace GarageFlow.Api.ServiceOrders.DTOs;

public sealed record PagedServiceOrderResponse(
    IReadOnlyList<ServiceOrderResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
