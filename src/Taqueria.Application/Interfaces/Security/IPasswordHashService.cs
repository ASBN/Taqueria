namespace Taqueria.Application.Interfaces.Security;

public interface IPasswordHashService
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
