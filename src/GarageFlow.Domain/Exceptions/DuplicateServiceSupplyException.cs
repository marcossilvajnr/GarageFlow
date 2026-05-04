namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateServiceSupplyException(string message) : DomainException(message);
