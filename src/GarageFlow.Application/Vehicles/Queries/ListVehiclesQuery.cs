namespace GarageFlow.Application.Vehicles.Queries;

public sealed record ListVehiclesQuery(
    Guid? CustomerId = null,
    int Page = 1,
    int PageSize = 10);
