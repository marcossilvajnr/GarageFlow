namespace GarageFlow.Application.Vehicles.Commands;

public sealed record CreateVehicleCommand(
    Guid CustomerId,
    string LicensePlate,
    string Renavam,
    string Make,
    string Model,
    int Year,
    string Color);
