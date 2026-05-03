namespace GarageFlow.Application.Vehicles.DTOs;

public sealed record VehicleDto(
    Guid Id,
    Guid CustomerId,
    string LicensePlate,
    string Renavam,
    string Make,
    string Model,
    int Year,
    string Color,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
