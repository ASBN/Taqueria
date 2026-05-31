using System.Security.Cryptography;

namespace Taqueria.Infrastructure.Security;
using Taqueria.Application.Interfaces.Security;

public sealed class PasswordHashService : IPasswordHashService
{
    private const int Iterations = 120_000;
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, HashSize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string password, string hash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hash))
        {
            return false;
        }

        var segments = hash.Split('.');
        if (segments.Length != 3 || !int.TryParse(segments[0], out var iterations))
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(segments[1]);
            var expected = Convert.FromBase64String(segments[2]);
            var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, expected.Length);
            return CryptographicOperations.FixedTimeEquals(actual, expected);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
