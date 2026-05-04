namespace GarageFlow.Domain.Exceptions;

public sealed class QuoteNotFoundException(string message) : DomainException(message);
