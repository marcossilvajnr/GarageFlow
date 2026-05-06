using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using GarageFlow.Infrastructure.Persistence;

namespace GarageFlow.Infrastructure.Auth;

public interface IAuthUserSeedService
{
    Task EnsureSeedAsync(CancellationToken cancellationToken = default);
}

internal sealed class AuthUserSeedService(
    GarageFlowDbContext dbContext,
    IOptions<List<AuthSeedUserOptions>> seedUsersOptions,
    PasswordHashService passwordHashService) : IAuthUserSeedService
{
    public async Task EnsureSeedAsync(CancellationToken cancellationToken = default)
    {
        if (await dbContext.AuthUsers.AnyAsync(cancellationToken))
            return;

        var configuredUsers = seedUsersOptions.Value
            .Where(u => !string.IsNullOrWhiteSpace(u.Username)
                        && !string.IsNullOrWhiteSpace(u.Password)
                        && !string.IsNullOrWhiteSpace(u.DisplayName)
                        && !string.IsNullOrWhiteSpace(u.Role))
            .ToList();

        if (configuredUsers.Count == 0)
            return;

        var users = configuredUsers.Select(user => new AuthUser
        {
            Id = Guid.NewGuid(),
            Username = user.Username.Trim().ToLowerInvariant(),
            DisplayName = user.DisplayName.Trim(),
            Role = user.Role.Trim(),
            PasswordHash = passwordHashService.Hash(user.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        });

        await dbContext.AuthUsers.AddRangeAsync(users, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
