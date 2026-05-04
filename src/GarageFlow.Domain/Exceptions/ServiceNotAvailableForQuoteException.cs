namespace GarageFlow.Domain.Exceptions;

public sealed class ServiceNotAvailableForQuoteException(string message) : DomainException(message);
