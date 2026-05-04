namespace GarageFlow.Domain.Exceptions;

public sealed class DiagnosticLastServiceException(string message) : DomainException(message);
