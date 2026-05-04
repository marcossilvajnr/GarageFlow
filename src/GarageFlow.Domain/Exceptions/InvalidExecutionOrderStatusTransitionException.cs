namespace GarageFlow.Domain.Exceptions;

public sealed class InvalidExecutionOrderStatusTransitionException(string message) : DomainException(message);
