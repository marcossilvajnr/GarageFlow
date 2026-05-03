namespace GarageFlow.Api.DTOs.Vehicles;

public sealed record PagedVehicleResponse(
    IReadOnlyList<VehicleResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
