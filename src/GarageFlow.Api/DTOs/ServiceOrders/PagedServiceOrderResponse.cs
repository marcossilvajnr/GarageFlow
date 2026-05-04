namespace GarageFlow.Api.DTOs.ServiceOrders;

public sealed record PagedServiceOrderResponse(
    IReadOnlyList<ServiceOrderResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
