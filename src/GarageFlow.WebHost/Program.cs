using System.Text;
using GarageFlow.Api;
using GarageFlow.Application;
using GarageFlow.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddGarageFlowApi(builder.Configuration);

var jwtIssuer = builder.Configuration["Jwt:Issuer"]!;
var jwtAudience = builder.Configuration["Jwt:Audience"]!;
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"]!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
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

var app = builder.Build();

await app.Services.RunInfrastructureStartupTasksAsync(app.Configuration);

if (app.Environment.IsDevelopment())
{
    app.UseGarageFlowApiDevelopmentExperience();
    app.MapGarageFlowDevelopmentEndpoints();
}

app.UseGarageFlowApiPipeline();
app.MapGarageFlowApiEndpoints();

app.Run();

public partial class Program { }
