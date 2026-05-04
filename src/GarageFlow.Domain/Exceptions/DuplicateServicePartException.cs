namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateServicePartException(string message) : DomainException(message);
