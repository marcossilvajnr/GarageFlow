namespace GarageFlow.Domain.Exceptions;

public sealed class DiagnosticAlreadyStartedException(string message) : DomainException(message);
