using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;

namespace GarageFlow.Api;

public static class GarageFlowApiApplicationBuilderExtensions
{
    public static WebApplication UseGarageFlowApiPipeline(this WebApplication app)
    {
        app.UseHttpsRedirection();
        app.UseExceptionHandler();
        app.UseAuthentication();
        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue)
                && !string.IsNullOrWhiteSpace(headerValue)
                ? headerValue.ToString()
                : context.TraceIdentifier;

            context.Response.Headers["X-Correlation-ID"] = correlationId;

            var actorId = context.User?.Identity?.IsAuthenticated == true
                ? context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                    ?? context.User.FindFirst("sub")?.Value
                : null;

            using var scope = app.Logger.BeginScope(new Dictionary<string, object?>
            {
                ["CorrelationId"] = correlationId,
                ["ActorId"] = actorId
            });

            app.Logger.LogInformation(
                "request_started method={Method} path={Path} correlationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path.Value,
                correlationId);

            await next();

            app.Logger.LogInformation(
                "request_completed method={Method} path={Path} statusCode={StatusCode} correlationId={CorrelationId}",
                context.Request.Method,
                context.Request.Path.Value,
                context.Response.StatusCode,
                correlationId);
        });
        app.UseAuthorization();

        return app;
    }

    public static WebApplication UseGarageFlowApiDevelopmentExperience(this WebApplication app)
    {
        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "GarageFlow API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
