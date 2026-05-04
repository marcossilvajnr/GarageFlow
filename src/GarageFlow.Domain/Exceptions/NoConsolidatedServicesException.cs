namespace GarageFlow.Domain.Exceptions;

public sealed class NoConsolidatedServicesException(string message) : DomainException(message);
