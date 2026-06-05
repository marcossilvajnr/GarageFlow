using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Api.ErrorHandling;

public static class ExceptionToProblemDetailsMapper
{
    public static ExceptionMapping Map(Exception exception) =>
        exception switch
        {
            InvalidCredentialsException => new(StatusCodes.Status401Unauthorized, "Não autorizado"),

            EntityNotFoundException or QuoteNotFoundException =>
                new(StatusCodes.Status404NotFound, "Não encontrado"),

            DuplicateDocumentException or
            DuplicatePartDataException or
            DuplicateServiceDataException or
            DuplicateSupplierDataException or
            DuplicateSupplyDataException or
            DuplicateVehicleDataException or
            DuplicateStockDataException or
            DuplicateServiceOrderServiceException or
            DuplicateDiagnosticServiceException or
            DuplicateServicePartException or
            DuplicateServiceSupplyException =>
                new(StatusCodes.Status409Conflict, "Conflito"),

            InvalidExecutionOrderStatusTransitionException or
            InvalidPurchaseOrderStatusTransitionException or
            InvalidSeparationOrderStatusTransitionException or
            InvalidServiceOrderStatusTransitionException =>
                new(StatusCodes.Status409Conflict, "Conflito de estado"),

            StockQuantityConflictException or
            DiagnosticLastServiceException or
            DiagnosticNoServicesException or
            DiagnosticNotCompletedException or
            NoConsolidatedServicesException or
            QuoteAlreadyExistsException or
            ServiceNotAvailableForQuoteException =>
                new(StatusCodes.Status409Conflict, "Conflito"),

            InvalidLoginPayloadException or
            DiagnosticAlreadyStartedException or
            DiagnosticNotInProgressException or
            QuoteAlreadyDecidedException or
            SeparationOrderCustodyPreconditionException or
            DomainException =>
                new(StatusCodes.Status400BadRequest, "Erro de validação"),

            _ => new(StatusCodes.Status500InternalServerError, "Erro interno")
        };
}
