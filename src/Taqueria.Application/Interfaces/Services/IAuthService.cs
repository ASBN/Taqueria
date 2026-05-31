namespace Taqueria.Application.Interfaces.Services;
using Taqueria.Application.DTOs.Auth;

public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> CambiarPasswordAsync(int usuarioId, CambiarPasswordDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioSesionDto> ObtenerUsuarioActualAsync(int usuarioId, CancellationToken cancellationToken = default);
}
