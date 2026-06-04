namespace GarageFlow.Api.Vehicles.DTOs;

public sealed record CreateVehicleRequest(
    Guid CustomerId,
    string LicensePlate,
    string Renavam,
    string Make,
    string Model,
    int Year,
    string Color);
