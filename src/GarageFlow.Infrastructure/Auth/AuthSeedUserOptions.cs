namespace GarageFlow.Infrastructure.Auth;

public sealed class AuthSeedUserOptions
{
    public const string SectionName = "Auth:SeedUsers";

    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
}
