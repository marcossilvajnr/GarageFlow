namespace GarageFlow.Application.Auth.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, string displayName, string role);
    int GetExpiresInSeconds();
}
