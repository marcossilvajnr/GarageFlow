namespace GarageFlow.Api.Auth.DTOs;

public sealed record LoginResponse(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string Role);
