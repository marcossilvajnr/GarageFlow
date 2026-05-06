using GarageFlow.Application.Auth.DTOs;

namespace GarageFlow.Application.Auth.Interfaces;

public interface IAuthUserCredentialStore
{
    Task<AuthUserCredentialDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
}
