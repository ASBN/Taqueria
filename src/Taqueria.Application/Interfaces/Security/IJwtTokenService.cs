namespace Taqueria.Application.Interfaces.Security;
using Taqueria.Domain.Entities;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiraUtc) CrearToken(Usuario usuario);
}
