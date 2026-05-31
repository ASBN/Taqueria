namespace Taqueria.Domain.Interfaces.Repositories;
using Taqueria.Domain.Entities;

public interface IPrecioProductoRepository
{
    Task<IReadOnlyList<PrecioProducto>> ObtenerPorProductoAsync(int productoId, CancellationToken cancellationToken = default);
    Task<PrecioProducto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PrecioProducto?> ObtenerVigenteAsync(int productoId, DateTime fechaUtc, CancellationToken cancellationToken = default);
    Task<bool> FueUtilizadoAsync(int precioId, CancellationToken cancellationToken = default);
    Task AgregarAsync(PrecioProducto precio, CancellationToken cancellationToken = default);
    void Eliminar(PrecioProducto precio);
}
