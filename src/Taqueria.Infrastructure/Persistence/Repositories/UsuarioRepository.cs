using Microsoft.EntityFrameworkCore;

namespace Taqueria.Infrastructure.Persistence.Repositories;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;
using Taqueria.Domain.Interfaces.Repositories;

public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly TaqueriaDbContext _context;

    public UsuarioRepository(TaqueriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Usuario>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        await _context.Usuarios.AsNoTracking()
            .OrderByDescending(x => x.Activo)
            .ThenBy(x => x.Rol)
            .ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

    public Task<Usuario?> ObtenerPorNombreUsuarioAsync(string nombreUsuario, CancellationToken cancellationToken = default) =>
        _context.Usuarios.FirstOrDefaultAsync(x => x.NombreUsuario == nombreUsuario, cancellationToken);

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Usuarios.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExisteNombreUsuarioAsync(
        string nombreUsuario,
        int? excluirId = null,
        CancellationToken cancellationToken = default) =>
        _context.Usuarios.AnyAsync(
            x => x.NombreUsuario == nombreUsuario && (!excluirId.HasValue || x.Id != excluirId.Value),
            cancellationToken);

    public Task<int> ContarActivosPorRolAsync(RolUsuario rol, CancellationToken cancellationToken = default) =>
        _context.Usuarios.CountAsync(x => x.Activo && x.Rol == rol, cancellationToken);

    public Task AgregarAsync(Usuario usuario, CancellationToken cancellationToken = default) =>
        _context.Usuarios.AddAsync(usuario, cancellationToken).AsTask();
}
