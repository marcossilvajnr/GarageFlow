namespace GarageFlow.Domain.Exceptions;

public sealed class StockQuantityConflictException(string message) : DomainException(message);
