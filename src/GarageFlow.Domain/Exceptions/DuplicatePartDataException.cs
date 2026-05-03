namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicatePartDataException(string message) : DomainException(message);
