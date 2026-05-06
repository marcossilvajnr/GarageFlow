using System.Text;
using GarageFlow.Api.Endpoints;
using GarageFlow.Api.Endpoints.Auth;
using GarageFlow.Api.Endpoints.Customers;
using GarageFlow.Api.Endpoints.Employees;
using GarageFlow.Api.Endpoints.Executions;
using GarageFlow.Api.Endpoints.Parts;
using GarageFlow.Api.Endpoints.Purchasing;
using GarageFlow.Api.Endpoints.ServiceOrders;
using GarageFlow.Api.Endpoints.Services;
using GarageFlow.Api.Endpoints.Stock;
using GarageFlow.Api.Endpoints.Suppliers;
using GarageFlow.Api.Endpoints.Supplies;
using GarageFlow.Api.Endpoints.Vehicles;
using GarageFlow.Api.Swagger;
using GarageFlow.Application;
using GarageFlow.Infrastructure;
using GarageFlow.Infrastructure.Auth;
using GarageFlow.Infrastructure.Persistence;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
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
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrative", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("Administrative"));
});

var app = builder.Build();

var autoMigrateOnStartup = app.Configuration.GetValue("Database:AutoMigrateOnStartup", true);

if (autoMigrateOnStartup)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<GarageFlowDbContext>();
    await dbContext.Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
    var authSeedService = scope.ServiceProvider.GetRequiredService<IAuthUserSeedService>();
    await authSeedService.EnsureSeedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GarageFlow API v1");
        options.RoutePrefix = "swagger";
    });
    app.MapDevelopmentDatabaseEndpoints();
}

app.UseHttpsRedirection();
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
app.MapHealthEndpoints();
app.MapAuthEndpoints();
app.MapCustomerEndpoints();
app.MapVehicleEndpoints();
app.MapSupplierEndpoints();
app.MapEmployeeEndpoints();
app.MapServiceEndpoints();
app.MapPartEndpoints();
app.MapSupplyEndpoints();
app.MapServiceOrderEndpoints();
app.MapStockEndpoints();
app.MapSeparationOrderEndpoints();
app.MapExecutionOrderEndpoints();
app.MapPurchaseOrderEndpoints();

app.Run();

public partial class Program { }
