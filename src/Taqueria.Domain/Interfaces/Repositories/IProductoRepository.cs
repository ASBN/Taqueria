namespace Taqueria.Domain.Interfaces.Repositories;
using Taqueria.Domain.Entities;

public interface IProductoRepository
{
    Task<IReadOnlyList<Producto>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<Producto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Producto?> ObtenerConPreciosAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken cancellationToken = default);
    Task<bool> FueVendidoAsync(int productoId, CancellationToken cancellationToken = default);
    Task AgregarAsync(Producto producto, CancellationToken cancellationToken = default);
    void Eliminar(Producto producto);
}
