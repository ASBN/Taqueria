namespace Taqueria.Domain.Interfaces.Repositories;
using Taqueria.Domain.Entities;

public interface ICategoriaProductoRepository
{
    Task<IReadOnlyList<CategoriaProducto>> ObtenerTodasAsync(CancellationToken cancellationToken = default);
    Task<CategoriaProducto?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken cancellationToken = default);
    Task<bool> TieneProductosAsync(int categoriaId, CancellationToken cancellationToken = default);
    Task AgregarAsync(CategoriaProducto categoria, CancellationToken cancellationToken = default);
    void Eliminar(CategoriaProducto categoria);
}
