using GarageFlow.Api.Endpoints;
using GarageFlow.Api.Endpoints.Services;
using GarageFlow.Application;
using GarageFlow.Infrastructure;
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
app.MapHealthEndpoints();
app.MapCustomerEndpoints();
app.MapVehicleEndpoints();
app.MapSupplierEndpoints();
app.MapEmployeeEndpoints();
app.MapServiceEndpoints();

app.Run();

public partial class Program { }
