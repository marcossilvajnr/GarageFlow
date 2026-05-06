using System.Text;
using GarageFlow.Api.Endpoints;
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
using GarageFlow.Application;
using GarageFlow.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "GarageFlow API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthEndpoints();
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
