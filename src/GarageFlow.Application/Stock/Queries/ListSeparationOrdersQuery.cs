namespace GarageFlow.Application.Stock.Queries;

public sealed record ListSeparationOrdersQuery(
    int Page = SeparationOrderPaginationDefaults.DefaultPage,
    int PageSize = SeparationOrderPaginationDefaults.DefaultPageSize);
