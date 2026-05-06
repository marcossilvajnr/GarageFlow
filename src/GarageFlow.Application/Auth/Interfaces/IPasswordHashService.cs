namespace GarageFlow.Application.Auth.Interfaces;

public interface IPasswordHashService
{
    bool Verify(string hashedPassword, string providedPassword);
}
