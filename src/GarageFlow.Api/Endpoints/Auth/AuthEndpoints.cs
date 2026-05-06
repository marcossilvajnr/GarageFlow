using GarageFlow.Api.DTOs.Auth;
using GarageFlow.Application.Auth.Commands;
using GarageFlow.Application.Auth.Handlers;
using GarageFlow.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GarageFlow.Api.Endpoints.Auth;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/auth").WithTags("Auth");

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .WithName("Login")
            .WithSummary("Autentica usuário e emite JWT.")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> Login(
        LoginRequest request,
        LoginHandler handler,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(
                new LoginCommand(request.Username, request.Password),
                cancellationToken);

            return Results.Ok(new LoginResponse(
                result.AccessToken,
                result.TokenType,
                result.ExpiresIn,
                result.Role));
        }
        catch (InvalidCredentialsException)
        {
            return Results.Unauthorized();
        }
        catch (InvalidLoginPayloadException ex)
        {
            return Results.BadRequest(new ProblemDetails
            {
                Title = "Erro de validação",
                Detail = ex.Message,
                Status = 400
            });
        }
    }
}
