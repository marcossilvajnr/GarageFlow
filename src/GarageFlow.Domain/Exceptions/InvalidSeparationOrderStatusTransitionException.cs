namespace GarageFlow.Domain.Exceptions;

public sealed class InvalidSeparationOrderStatusTransitionException(string message) : DomainException(message);
