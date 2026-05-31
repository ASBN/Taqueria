namespace Taqueria.Application.Interfaces.Services;
using Taqueria.Application.DTOs.Usuarios;

public interface IUsuarioService
{
    Task<IReadOnlyList<UsuarioDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<UsuarioDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto, CancellationToken cancellationToken = default);
    Task<UsuarioDto> ActualizarAsync(int id, ActualizarUsuarioDto dto, int usuarioActualId, CancellationToken cancellationToken = default);
    Task<UsuarioDto> RestablecerPasswordAsync(int id, RestablecerPasswordDto dto, CancellationToken cancellationToken = default);
    Task DesactivarAsync(int id, int usuarioActualId, CancellationToken cancellationToken = default);
}
