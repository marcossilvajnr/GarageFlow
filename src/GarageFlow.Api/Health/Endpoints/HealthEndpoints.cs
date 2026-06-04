namespace GarageFlow.Api.Health.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new
        {
            status = "ok",
            service = "GarageFlow.Api"
        }))
        .WithName("HealthCheck")
        .WithSummary("Verifica se a API esta ativa.")
        .WithDescription("Endpoint tecnico para validacao rapida de disponibilidade da API.")
        .WithTags("Health")
        .Produces(StatusCodes.Status200OK);

        return endpoints;
    }
}
