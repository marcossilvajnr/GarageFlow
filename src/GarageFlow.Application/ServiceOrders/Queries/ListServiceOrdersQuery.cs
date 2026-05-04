namespace GarageFlow.Application.ServiceOrders.Queries;

public sealed record ListServiceOrdersQuery(
    int Page = ServiceOrdersPaginationDefaults.DefaultPage,
    int PageSize = ServiceOrdersPaginationDefaults.DefaultPageSize);
