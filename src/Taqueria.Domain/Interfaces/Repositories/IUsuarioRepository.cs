namespace Taqueria.Domain.Interfaces.Repositories;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;

public interface IUsuarioRepository
{
    Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default);
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreUsuarioAsync(string nombreUsuario, int? excluirId = null, CancellationToken cancellationToken = default);
    Task<int> ContarActivosPorRolAsync(RolUsuario rol, CancellationToken cancellationToken = default);
    Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
