namespace Taqueria.Infrastructure.Persistence;
using Taqueria.Domain.Interfaces;
using Taqueria.Domain.Interfaces.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TaqueriaDbContext _context;

    public UnitOfWork(
        TaqueriaDbContext context,
        IUsuarioRepository usuarios,
        IMesaRepository mesas,
        IComandaRepository comandas,
        IProductoRepository productos,
        ICategoriaProductoRepository categoriasProducto,
        IPrecioProductoRepository preciosProducto)
    {
        _context = context;
        Usuarios = usuarios;
        Mesas = mesas;
        Comandas = comandas;
        Productos = productos;
        CategoriasProducto = categoriasProducto;
        PreciosProducto = preciosProducto;
    }

    public IUsuarioRepository Usuarios { get; }
    public IMesaRepository Mesas { get; }
    public IComandaRepository Comandas { get; }
    public IProductoRepository Productos { get; }
    public ICategoriaProductoRepository CategoriasProducto { get; }
    public IPrecioProductoRepository PreciosProducto { get; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        _context.SaveChangesAsync(cancellationToken);
}
