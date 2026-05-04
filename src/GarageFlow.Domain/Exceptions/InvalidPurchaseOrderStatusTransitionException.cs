namespace GarageFlow.Domain.Exceptions;

public sealed class InvalidPurchaseOrderStatusTransitionException(string message) : DomainException(message);
