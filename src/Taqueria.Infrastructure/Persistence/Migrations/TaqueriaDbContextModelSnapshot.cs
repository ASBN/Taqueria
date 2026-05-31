using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
#nullable disable

namespace Taqueria.Infrastructure.Persistence.Migrations;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;

[DbContext(typeof(TaqueriaDbContext))]
partial class TaqueriaDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

        modelBuilder.Entity<CategoriaProducto>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<bool>("Activa");
            b.Property<string>("Nombre").IsRequired().HasMaxLength(80);
            b.Property<int>("OrdenVisual");
            b.HasKey("Id");
            b.HasIndex("Nombre").IsUnique();
            b.ToTable("CategoriasProducto");
        });

        modelBuilder.Entity<Mesa>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<bool>("Activa");
            b.Property<int>("Capacidad");
            b.Property<int>("Numero");
            b.HasKey("Id");
            b.HasIndex("Numero").IsUnique();
            b.ToTable("Mesas");
        });

        modelBuilder.Entity<Usuario>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<bool>("Activo");
            b.Property<bool>("DebeCambiarPassword");
            b.Property<DateTime>("FechaCreacionUtc");
            b.Property<string>("Nombre").IsRequired().HasMaxLength(120);
            b.Property<string>("NombreUsuario").IsRequired().HasMaxLength(80);
            b.Property<string>("PasswordHash").IsRequired().HasMaxLength(500);
            b.Property<RolUsuario>("Rol");
            b.HasKey("Id");
            b.HasIndex("NombreUsuario").IsUnique();
            b.ToTable("Usuarios");
        });

        modelBuilder.Entity<Producto>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<bool>("Activo");
            b.Property<int>("CategoriaProductoId");
            b.Property<string>("Nombre").IsRequired().HasMaxLength(120);
            b.Property<int>("OrdenVisual");
            b.HasKey("Id");
            b.HasIndex("CategoriaProductoId");
            b.HasIndex("Nombre").IsUnique();
            b.ToTable("Productos");
        });

        modelBuilder.Entity<PrecioProducto>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<DateTime>("FechaCreacionUtc");
            b.Property<decimal>("Precio").HasPrecision(10, 2);
            b.Property<int>("ProductoId");
            b.Property<DateTime>("VigenteDesdeUtc");
            b.HasKey("Id");
            b.HasIndex("ProductoId", "VigenteDesdeUtc").IsUnique();
            b.ToTable("PreciosProducto");
        });

        modelBuilder.Entity<Comanda>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<EstadoComanda>("Estado");
            b.Property<DateTime>("FechaHoraAperturaUtc");
            b.Property<DateTime?>("FechaHoraCancelacionUtc");
            b.Property<DateTime?>("FechaHoraCobroUtc");
            b.Property<DateTime?>("FechaHoraEntregaUtc");
            b.Property<DateTime?>("FechaHoraEnvioCocinaUtc");
            b.Property<string>("Folio").IsRequired().HasMaxLength(30);
            b.Property<int>("MesaId");
            b.Property<string>("MotivoCancelacion").HasMaxLength(300);
            b.Property<decimal?>("TotalCobrado").HasPrecision(10, 2);
            b.Property<int>("UsuarioMeseroId");
            b.Property<int?>("UsuarioCancelacionId");
            b.HasKey("Id");
            b.HasIndex("Folio").IsUnique();
            b.HasIndex("Estado", "FechaHoraCobroUtc");
            b.HasIndex("MesaId");
            b.HasIndex("UsuarioMeseroId");
            b.HasIndex("UsuarioCancelacionId");
            b.ToTable("Comandas");
        });

        modelBuilder.Entity<Comensal>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<bool>("Activo");
            b.Property<int>("ComandaId");
            b.Property<int>("Numero");
            b.HasKey("Id");
            b.HasIndex("ComandaId", "Numero").IsUnique();
            b.ToTable("Comensales");
        });

        modelBuilder.Entity<ComandaDetalle>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<int>("Cantidad");
            b.Property<int>("CantidadEntregada");
            b.Property<int>("CantidadPreparada");
            b.Property<int>("CategoriaProductoIdHistorico");
            b.Property<int>("ComensalId");
            b.Property<DateTime>("FechaHoraCapturaUtc");
            b.Property<string>("NombreCategoriaHistorico").IsRequired().HasMaxLength(80);
            b.Property<string>("NombreProductoHistorico").IsRequired().HasMaxLength(120);
            b.Property<decimal>("PrecioUnitarioHistorico").HasPrecision(10, 2);
            b.Property<int>("ProductoId");
            b.Property<decimal>("Subtotal").HasPrecision(10, 2);
            b.HasKey("Id");
            b.HasIndex("ComensalId");
            b.HasIndex("ProductoId");
            b.ToTable("ComandaDetalles");
        });

        modelBuilder.Entity<EntregaParcial>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<int>("Cantidad");
            b.Property<int>("ComandaDetalleId");
            b.Property<EstadoEntregaParcial>("Estado");
            b.Property<DateTime?>("FechaHoraEntregaUtc");
            b.Property<DateTime>("FechaHoraListaUtc");
            b.Property<int>("UsuarioCocinaId");
            b.Property<int?>("UsuarioMeseroEntregaId");
            b.HasKey("Id");
            b.HasIndex("ComandaDetalleId");
            b.HasIndex("UsuarioCocinaId");
            b.HasIndex("UsuarioMeseroEntregaId");
            b.ToTable("EntregasParciales");
        });

        modelBuilder.Entity<PagoComanda>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<int>("ComandaId");
            b.Property<DateTime>("FechaHoraPagoUtc");
            b.Property<decimal>("Importe").HasPrecision(10, 2);
            b.Property<MetodoPago>("MetodoPago");
            b.Property<int>("UsuarioCobroId");
            b.HasKey("Id");
            b.HasIndex("ComandaId");
            b.HasIndex("UsuarioCobroId");
            b.ToTable("PagosComanda");
        });

        modelBuilder.Entity<MermaComandaDetalle>(b =>
        {
            b.Property<int>("Id").ValueGeneratedOnAdd();
            b.Property<int>("Cantidad");
            b.Property<int>("ComandaDetalleId");
            b.Property<DateTime>("FechaHoraRegistroUtc");
            b.Property<decimal>("ImporteHistorico").HasPrecision(10, 2);
            b.Property<string>("Motivo").IsRequired().HasMaxLength(300);
            b.Property<int>("UsuarioRegistroId");
            b.HasKey("Id");
            b.HasIndex("ComandaDetalleId");
            b.HasIndex("UsuarioRegistroId");
            b.ToTable("MermasComandaDetalle");
        });

        modelBuilder.Entity<Producto>()
            .HasOne(x => x.CategoriaProducto)
            .WithMany(x => x.Productos)
            .HasForeignKey(x => x.CategoriaProductoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<PrecioProducto>()
            .HasOne(x => x.Producto)
            .WithMany(x => x.Precios)
            .HasForeignKey(x => x.ProductoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<Comanda>()
            .HasOne(x => x.Mesa)
            .WithMany(x => x.Comandas)
            .HasForeignKey(x => x.MesaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<Comanda>()
            .HasOne(x => x.UsuarioMesero)
            .WithMany()
            .HasForeignKey(x => x.UsuarioMeseroId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<Comanda>()
            .HasOne(x => x.UsuarioCancelacion)
            .WithMany()
            .HasForeignKey(x => x.UsuarioCancelacionId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Comensal>()
            .HasOne(x => x.Comanda)
            .WithMany(x => x.Comensales)
            .HasForeignKey(x => x.ComandaId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<ComandaDetalle>()
            .HasOne(x => x.Comensal)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.ComensalId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<ComandaDetalle>()
            .HasOne(x => x.Producto)
            .WithMany()
            .HasForeignKey(x => x.ProductoId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<EntregaParcial>()
            .HasOne(x => x.ComandaDetalle)
            .WithMany(x => x.EntregasParciales)
            .HasForeignKey(x => x.ComandaDetalleId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        modelBuilder.Entity<EntregaParcial>()
            .HasOne(x => x.UsuarioCocina)
            .WithMany()
            .HasForeignKey(x => x.UsuarioCocinaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<EntregaParcial>()
            .HasOne(x => x.UsuarioMeseroEntrega)
            .WithMany()
            .HasForeignKey(x => x.UsuarioMeseroEntregaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<PagoComanda>()
            .HasOne(x => x.Comanda)
            .WithMany(x => x.Pagos)
            .HasForeignKey(x => x.ComandaId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<MermaComandaDetalle>()
            .HasOne(x => x.ComandaDetalle)
            .WithMany(x => x.Mermas)
            .HasForeignKey(x => x.ComandaDetalleId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<MermaComandaDetalle>()
            .HasOne(x => x.UsuarioRegistro)
            .WithMany()
            .HasForeignKey(x => x.UsuarioRegistroId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<PagoComanda>()
            .HasOne(x => x.UsuarioCobro)
            .WithMany()
            .HasForeignKey(x => x.UsuarioCobroId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}
