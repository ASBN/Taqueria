using Microsoft.EntityFrameworkCore;

namespace Taqueria.Infrastructure.Persistence;
using Taqueria.Domain.Entities;

public sealed class TaqueriaDbContext : DbContext
{
    public TaqueriaDbContext(DbContextOptions<TaqueriaDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<CategoriaProducto> CategoriasProducto => Set<CategoriaProducto>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<PrecioProducto> PreciosProducto => Set<PrecioProducto>();
    public DbSet<Comanda> Comandas => Set<Comanda>();
    public DbSet<Comensal> Comensales => Set<Comensal>();
    public DbSet<ComandaDetalle> ComandaDetalles => Set<ComandaDetalle>();
    public DbSet<EntregaParcial> EntregasParciales => Set<EntregaParcial>();
    public DbSet<PagoComanda> PagosComanda => Set<PagoComanda>();
    public DbSet<MermaComandaDetalle> MermasComandaDetalle => Set<MermaComandaDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TaqueriaDbContext).Assembly);
    }
}
