namespace GarageFlow.Domain.Exceptions;

public sealed class ExternalQuoteDecisionConflictException(string message) : DomainException(message);
