using GarageFlow.Domain.Exceptions;

namespace GarageFlow.Application.Common.Errors;

public static class ApplicationExceptionMapper
{
    public static ApplicationErrorDescriptor Map(Exception exception) =>
        exception switch
        {
            InvalidCredentialsException => new(ApplicationErrorKind.Unauthorized),

            EntityNotFoundException or QuoteNotFoundException =>
                new(ApplicationErrorKind.NotFound),

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
                new(ApplicationErrorKind.Conflict),

            InvalidExecutionOrderStatusTransitionException or
            InvalidPurchaseOrderStatusTransitionException or
            InvalidSeparationOrderStatusTransitionException or
            InvalidServiceOrderStatusTransitionException =>
                new(ApplicationErrorKind.StateConflict),

            StockQuantityConflictException or
            DiagnosticLastServiceException or
            DiagnosticNoServicesException or
            DiagnosticNotCompletedException or
            NoConsolidatedServicesException or
            QuoteAlreadyExistsException or
            ServiceNotAvailableForQuoteException or
            ExternalQuoteDecisionConflictException =>
                new(ApplicationErrorKind.Conflict),

            InvalidLoginPayloadException or
            DiagnosticAlreadyStartedException or
            DiagnosticNotInProgressException or
            QuoteAlreadyDecidedException or
            SeparationOrderCustodyPreconditionException or
            DomainException =>
                new(ApplicationErrorKind.Validation),

            _ => new(ApplicationErrorKind.Unexpected)
        };
}
