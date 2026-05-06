namespace GarageFlow.Domain.Exceptions;

public sealed class InvalidLoginPayloadException(string message) : DomainException(message);
