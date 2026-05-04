namespace GarageFlow.Domain.Exceptions;

public sealed class QuoteAlreadyDecidedException(string message) : DomainException(message);
