namespace Taqueria.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; set; } = "Taqueria.Api";
    public string Audience { get; set; } = "Taqueria.Web";
    public string Key { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 480;
}
