namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateExecutionOrderException(string message) : DomainException(message);
