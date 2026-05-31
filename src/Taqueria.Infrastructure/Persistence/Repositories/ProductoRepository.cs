using Microsoft.EntityFrameworkCore;

namespace Taqueria.Infrastructure.Persistence.Repositories;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces.Repositories;

public sealed class ProductoRepository : IProductoRepository
{
    private readonly TaqueriaDbContext _context;

    public ProductoRepository(TaqueriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Producto>> ObtenerTodosAsync(CancellationToken cancellationToken = default) =>
        await _context.Productos.AsNoTracking()
            .Include(x => x.CategoriaProducto)
            .Include(x => x.Precios)
            .OrderBy(x => x.CategoriaProducto!.OrdenVisual)
            .ThenBy(x => x.OrdenVisual)
            .ThenBy(x => x.Nombre)
            .ToListAsync(cancellationToken);

    public Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Productos.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<Producto?> ObtenerConPreciosAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Productos
            .Include(x => x.CategoriaProducto)
            .Include(x => x.Precios)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken cancellationToken = default)
    {
        var nombreNormalizado = nombre.Trim().ToLower();
        return _context.Productos.AnyAsync(
            x => x.Nombre.ToLower() == nombreNormalizado && (!excluirId.HasValue || x.Id != excluirId.Value),
            cancellationToken);
    }

    public Task<bool> FueVendidoAsync(int productoId, CancellationToken cancellationToken = default) =>
        _context.ComandaDetalles.AnyAsync(x => x.ProductoId == productoId, cancellationToken);

    public Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default) =>
        _context.Productos.AddAsync(producto, cancellationToken).AsTask();

    public void Eliminar(Producto producto) => _context.Productos.Remove(producto);
}
