namespace GarageFlow.Api.Vehicles.DTOs;

public sealed record UpdateVehicleRequest(
    string Make,
    string Model,
    int Year,
    string Color);
