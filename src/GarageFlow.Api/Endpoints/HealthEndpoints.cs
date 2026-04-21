namespace GarageFlow.Api.Endpoints;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health", () => Results.Ok(new
        {
            status = "ok",
            service = "GarageFlow.Api"
        }))
        .WithName("HealthCheck");

        return endpoints;
    }
}
