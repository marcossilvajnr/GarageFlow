using GarageFlow.Application.Common.Errors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MvcProblemDetails = Microsoft.AspNetCore.Mvc.ProblemDetails;

namespace GarageFlow.Api.Common.ErrorHandling;

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

        var error = ApplicationExceptionMapper.Map(exception);
        var mapping = ExceptionToProblemDetailsMapper.Map(error);
        httpContext.Response.StatusCode = mapping.StatusCode;

        var problemDetails = new MvcProblemDetails
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

    private static string GetCorrelationId(HttpContext httpContext) =>
        httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue)
            && !string.IsNullOrWhiteSpace(headerValue)
            ? headerValue.ToString()
            : httpContext.TraceIdentifier;
}
