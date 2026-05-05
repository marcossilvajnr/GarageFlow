namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateStockDataException(string message) : DomainException(message);
