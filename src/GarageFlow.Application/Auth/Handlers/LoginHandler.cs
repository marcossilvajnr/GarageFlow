using GarageFlow.Application.Auth.Commands;
using GarageFlow.Application.Auth.DTOs;
using GarageFlow.Application.Auth.Interfaces;
using GarageFlow.Domain.Exceptions;
using GarageFlow.Domain.Shared;

namespace GarageFlow.Application.Auth.Handlers;

public sealed class LoginHandler(
    IAuthUserCredentialStore authUserCredentialStore,
    IPasswordHashService passwordHashService,
    IJwtTokenGenerator jwtTokenGenerator)
{
    public async Task<LoginResultDto> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var username = command.Username?.Trim() ?? string.Empty;
        var password = command.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new InvalidLoginPayloadException(DomainErrorMessages.AuthInvalidLoginPayload);

        var credential = await authUserCredentialStore.GetByUsernameAsync(username, cancellationToken);
        if (credential is null || !credential.IsActive)
            throw new InvalidCredentialsException(DomainErrorMessages.AuthInvalidCredentials);

        var isValidPassword = passwordHashService.Verify(credential.PasswordHash, password);
        if (!isValidPassword)
            throw new InvalidCredentialsException(DomainErrorMessages.AuthInvalidCredentials);

        var accessToken = jwtTokenGenerator.GenerateToken(credential.Id, credential.DisplayName, credential.Role);

        return new LoginResultDto(
            accessToken,
            "Bearer",
            jwtTokenGenerator.GetExpiresInSeconds(),
            credential.Role);
    }
}
