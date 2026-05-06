namespace GarageFlow.Api.DTOs.Auth;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string Role);
