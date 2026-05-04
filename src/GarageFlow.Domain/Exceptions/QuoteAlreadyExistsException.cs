namespace GarageFlow.Domain.Exceptions;

public sealed class QuoteAlreadyExistsException(string message) : DomainException(message);
