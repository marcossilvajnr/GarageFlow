namespace GarageFlow.Application.Auth.DTOs;

public sealed record AuthUserCredentialDto(
    Guid Id,
    string Username,
    string DisplayName,
    string Role,
    string PasswordHash,
    bool IsActive);
