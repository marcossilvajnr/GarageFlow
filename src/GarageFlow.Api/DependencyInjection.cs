using GarageFlow.Api.Common.ErrorHandling;
using GarageFlow.Api.Swagger;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace GarageFlow.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddGarageFlowApi(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();
        services.AddProblemDetails();
        services.AddExceptionHandler<GarageFlowExceptionHandler>();
        services.AddSwaggerGen(options =>
        {
            // Prevent schema id collisions for DTOs with the same type name in different namespaces.
            options.CustomSchemaIds(type => type.FullName!.Replace("+", "."));
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "GarageFlow API",
                Version = "v1",
                Description = "API base do projeto GarageFlow para desenvolvimento e validacao tecnica."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Informe o token JWT no formato: {token}"
            });

            options.OperationFilter<AuthorizeOperationFilter>();
        });
        services.AddAuthorization();

        return services;
    }
}
