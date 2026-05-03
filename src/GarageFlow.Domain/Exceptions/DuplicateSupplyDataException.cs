namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateSupplyDataException(string message) : DomainException(message);
