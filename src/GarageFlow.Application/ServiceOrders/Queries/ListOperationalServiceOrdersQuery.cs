namespace GarageFlow.Application.ServiceOrders.Queries;

public sealed record ListOperationalServiceOrdersQuery(
    int Page = ServiceOrdersPaginationDefaults.DefaultPage,
    int PageSize = ServiceOrdersPaginationDefaults.DefaultPageSize);
