using GarageFlow.Application.Auth.DTOs;
using GarageFlow.Application.Auth.Interfaces;
using GarageFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GarageFlow.Infrastructure.Auth;

internal sealed class AuthUserCredentialStore(GarageFlowDbContext dbContext) : IAuthUserCredentialStore
{
    public async Task<AuthUserCredentialDto?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.Trim().ToLowerInvariant();

        return await dbContext.AuthUsers
            .AsNoTracking()
            .Where(u => u.Username == normalizedUsername)
            .Select(u => new AuthUserCredentialDto(
                u.Id,
                u.Username,
                u.DisplayName,
                u.Role,
                u.PasswordHash,
                u.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }
}
