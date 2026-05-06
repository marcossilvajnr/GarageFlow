namespace GarageFlow.Application.Auth.DTOs;

public sealed record LoginResultDto(
    string AccessToken,
    string TokenType,
    int ExpiresIn,
    string Role);
