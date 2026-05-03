namespace GarageFlow.Api.DTOs.Vehicles;

public sealed record CreateVehicleRequest(
    Guid CustomerId,
    string LicensePlate,
    string Renavam,
    string Make,
    string Model,
    int Year,
    string Color);
