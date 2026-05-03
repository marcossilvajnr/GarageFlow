namespace GarageFlow.Domain.Exceptions;

public sealed class EntityNotFoundException(string message) : DomainException(message);
