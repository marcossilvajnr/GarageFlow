namespace GarageFlow.Domain.Exceptions;

public sealed class DuplicateServiceOrderServiceException(string message) : DomainException(message);
