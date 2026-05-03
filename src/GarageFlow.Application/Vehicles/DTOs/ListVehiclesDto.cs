namespace GarageFlow.Application.Vehicles.DTOs;

public sealed record ListVehiclesDto(
    IReadOnlyList<VehicleDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
