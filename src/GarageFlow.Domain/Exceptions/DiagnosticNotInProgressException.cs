namespace GarageFlow.Domain.Exceptions;

public sealed class DiagnosticNotInProgressException(string message) : DomainException(message);
