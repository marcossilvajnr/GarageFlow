namespace GarageFlow.Domain.Exceptions;

public sealed class InvalidCredentialsException(string message) : DomainException(message);
