namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateServiceDataException(string message) : DomainException(message);
