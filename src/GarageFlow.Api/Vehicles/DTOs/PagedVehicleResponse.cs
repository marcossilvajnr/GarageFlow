namespace GarageFlow.Api.Vehicles.DTOs;

public sealed record PagedVehicleResponse(
    IReadOnlyList<VehicleResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
