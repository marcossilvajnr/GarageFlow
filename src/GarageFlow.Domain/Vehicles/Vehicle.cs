using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;
using GarageFlow.Domain.ValueObjects;

namespace GarageFlow.Domain.Vehicles;

public sealed class Vehicle
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public LicensePlate LicensePlate { get; private set; } = null!;
    public Renavam Renavam { get; private set; } = null!;
    public string Make { get; private set; } = string.Empty;
    public string Model { get; private set; } = string.Empty;
    public int Year { get; private set; }
    public string Color { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Vehicle() { }

    public static Vehicle Create(
        Guid customerId,
        string licensePlate,
        string renavam,
        string make,
        string model,
        int year,
        string color)
    {
        if (customerId == Guid.Empty)
            throw new DomainException(DomainErrorMessages.InvalidCustomerId);

        var trimmedMake = make?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedMake))
            throw new DomainException(DomainErrorMessages.InvalidVehicleMake);

        var trimmedModel = model?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedModel))
            throw new DomainException(DomainErrorMessages.InvalidVehicleModel);

        if (year < 1900 || year > DateTime.UtcNow.Year + 1)
            throw new DomainException(DomainErrorMessages.InvalidVehicleYear);

        var trimmedColor = color?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedColor))
            throw new DomainException(DomainErrorMessages.InvalidVehicleColor);

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            LicensePlate = LicensePlate.Create(licensePlate),
            Renavam = Renavam.Create(renavam),
            Make = trimmedMake,
            Model = trimmedModel,
            Year = year,
            Color = trimmedColor,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return vehicle;
    }

    public void Update(
        string make,
        string model,
        int year,
        string color)
    {
        var trimmedMake = make?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedMake))
            throw new DomainException(DomainErrorMessages.InvalidVehicleMake);

        var trimmedModel = model?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedModel))
            throw new DomainException(DomainErrorMessages.InvalidVehicleModel);

        if (year < 1900 || year > DateTime.UtcNow.Year + 1)
            throw new DomainException(DomainErrorMessages.InvalidVehicleYear);

        var trimmedColor = color?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(trimmedColor))
            throw new DomainException(DomainErrorMessages.InvalidVehicleColor);

        Make = trimmedMake;
        Model = trimmedModel;
        Year = year;
        Color = trimmedColor;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException(DomainErrorMessages.VehicleAlreadyInactive);

        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
