namespace GarageFlow.Domain.Exceptions;

public sealed class InvalidServiceOrderStatusTransitionException(string message) : DomainException(message);
