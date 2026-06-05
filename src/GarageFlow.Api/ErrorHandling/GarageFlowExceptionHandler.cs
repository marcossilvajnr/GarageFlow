using GarageFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.ErrorHandling;

public sealed class GarageFlowExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GarageFlowExceptionHandler> logger) : IExceptionHandler
{
    private const string GenericErrorMessage = "Ocorreu um erro inesperado.";

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (httpContext.Response.HasStarted)
        {
            return false;
        }

        var mapping = MapException(exception);
        httpContext.Response.StatusCode = mapping.StatusCode;

        var problemDetails = new ProblemDetails
        {
            Status = mapping.StatusCode,
            Title = mapping.Title,
            Detail = mapping.StatusCode == StatusCodes.Status500InternalServerError
                ? GenericErrorMessage
                : exception.Message,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;
        problemDetails.Extensions["correlationId"] = GetCorrelationId(httpContext);

        if (mapping.StatusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception, "unhandled_exception traceId={TraceId}", httpContext.TraceIdentifier);
        }
        else
        {
            logger.LogWarning(
                exception,
                "handled_exception statusCode={StatusCode} traceId={TraceId}",
                mapping.StatusCode,
                httpContext.TraceIdentifier);
        }

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails
        });
    }

    private static ExceptionMapping MapException(Exception exception) =>
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

    private static string GetCorrelationId(HttpContext httpContext) =>
        httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue.ToString()
            : httpContext.TraceIdentifier;

    private sealed record ExceptionMapping(int StatusCode, string Title);
}
