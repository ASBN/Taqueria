namespace Taqueria.Domain.Interfaces;
using Taqueria.Domain.Interfaces.Repositories;

public interface IUnitOfWork
{
    IUsuarioRepository Usuarios { get; }
    IMesaRepository Mesas { get; }
    IComandaRepository Comandas { get; }
    IProductoRepository Productos { get; }
    ICategoriaProductoRepository CategoriasProducto { get; }
    IPrecioProductoRepository PreciosProducto { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
