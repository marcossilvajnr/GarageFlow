using GarageFlow.Application.Auth.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace GarageFlow.Infrastructure.Auth;

internal sealed class PasswordHashService : IPasswordHashService
{
    private readonly PasswordHasher<AuthUser> _passwordHasher = new();

    public bool Verify(string hashedPassword, string providedPassword)
    {
        var result = _passwordHasher.VerifyHashedPassword(new AuthUser(), hashedPassword, providedPassword);
        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    public string Hash(string password)
        => _passwordHasher.HashPassword(new AuthUser(), password);
}
