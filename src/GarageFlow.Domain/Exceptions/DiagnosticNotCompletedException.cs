namespace GarageFlow.Domain.Exceptions;

public sealed class DiagnosticNotCompletedException(string message) : DomainException(message);
