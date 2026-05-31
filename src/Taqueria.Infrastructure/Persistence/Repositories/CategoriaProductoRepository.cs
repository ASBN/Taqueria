using Microsoft.EntityFrameworkCore;

namespace Taqueria.Infrastructure.Persistence.Repositories;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces.Repositories;

public sealed class CategoriaProductoRepository : ICategoriaProductoRepository
{
    private readonly TaqueriaDbContext _context;

    public CategoriaProductoRepository(TaqueriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<CategoriaProducto>> ObtenerTodasAsync(CancellationToken cancellationToken = default) =>
        await _context.CategoriasProducto.AsNoTracking()
            .OrderBy(x => x.OrdenVisual).ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

    public Task<CategoriaProducto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.CategoriasProducto.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken cancellationToken = default)
    {
        var nombreNormalizado = nombre.Trim().ToLower();
        return _context.CategoriasProducto.AnyAsync(
            x => x.Nombre.ToLower() == nombreNormalizado && (!excluirId.HasValue || x.Id != excluirId.Value),
            cancellationToken);
    }

    public Task<bool> TieneProductosAsync(int categoriaId, CancellationToken cancellationToken = default) =>
        _context.Productos.AnyAsync(x => x.CategoriaProductoId == categoriaId, cancellationToken);

    public Task AgregarAsync(CategoriaProducto categoria, CancellationToken cancellationToken = default) =>
        _context.CategoriasProducto.AddAsync(categoria, cancellationToken).AsTask();

    public void Eliminar(CategoriaProducto categoria) => _context.CategoriasProducto.Remove(categoria);
}
