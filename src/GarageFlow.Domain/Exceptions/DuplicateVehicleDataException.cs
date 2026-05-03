namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateVehicleDataException(string message) : DomainException(message);
