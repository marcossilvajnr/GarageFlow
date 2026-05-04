namespace GarageFlow.Domain.Exceptions;

public sealed class DiagnosticNoServicesException(string message) : DomainException(message);
