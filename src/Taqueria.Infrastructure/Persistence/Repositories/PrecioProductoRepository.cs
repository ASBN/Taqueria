using Microsoft.EntityFrameworkCore;

namespace Taqueria.Infrastructure.Persistence.Repositories;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces.Repositories;

public sealed class PrecioProductoRepository : IPrecioProductoRepository
{
    private readonly TaqueriaDbContext _context;

    public PrecioProductoRepository(TaqueriaDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<PrecioProducto>> ObtenerPorProductoAsync(int productoId, CancellationToken cancellationToken = default) =>
        await _context.PreciosProducto.AsNoTracking()
            .Where(x => x.ProductoId == productoId)
            .OrderByDescending(x => x.VigenteDesdeUtc)
            .ToListAsync(cancellationToken);

    public Task<PrecioProducto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.PreciosProducto.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<PrecioProducto?> ObtenerVigenteAsync(int productoId, DateTime fechaUtc, CancellationToken cancellationToken = default) =>
        _context.PreciosProducto.AsNoTracking()
            .Where(x => x.ProductoId == productoId && x.VigenteDesdeUtc <= fechaUtc)
            .OrderByDescending(x => x.VigenteDesdeUtc)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<bool> FueUtilizadoAsync(int precioId, CancellationToken cancellationToken = default)
    {
        var precio = await _context.PreciosProducto.AsNoTracking().FirstOrDefaultAsync(x => x.Id == precioId, cancellationToken);
        if (precio is null)
        {
            return false;
        }

        return await _context.ComandaDetalles.AnyAsync(
            x => x.ProductoId == precio.ProductoId && x.FechaHoraCapturaUtc >= precio.VigenteDesdeUtc,
            cancellationToken);
    }

    public Task AgregarAsync(PrecioProducto precio, CancellationToken cancellationToken = default) =>
        _context.PreciosProducto.AddAsync(precio, cancellationToken).AsTask();

    public void Eliminar(PrecioProducto precio) => _context.PreciosProducto.Remove(precio);
}
