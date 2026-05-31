using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Taqueria.Infrastructure.Security;
using Taqueria.Application.Interfaces.Security;
using Taqueria.Domain.Entities;

public sealed class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string Token, DateTime ExpiraUtc) CrearToken(Usuario usuario)
    {
        var expiraUtc = DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NombreUsuario),
            new Claim("nombre", usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol.ToString()),
            new Claim("debe_cambiar_password", usuario.DebeCambiarPassword ? "true" : "false")
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expiraUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiraUtc);
    }
}
