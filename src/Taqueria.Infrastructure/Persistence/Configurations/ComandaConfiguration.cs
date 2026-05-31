using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taqueria.Infrastructure.Persistence.Configurations;
using Taqueria.Domain.Entities;

public sealed class ComandaConfiguration : IEntityTypeConfiguration<Comanda>
{
    public void Configure(EntityTypeBuilder<Comanda> builder)
    {
        builder.ToTable("Comandas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Folio).HasMaxLength(30).IsRequired();
        builder.HasIndex(x => x.Folio).IsUnique();
        builder.HasIndex(x => new { x.Estado, x.FechaHoraCobroUtc });
        builder.Property(x => x.MotivoCancelacion).HasMaxLength(300);
        builder.Property(x => x.TotalCobrado).HasPrecision(10, 2);
        builder.HasOne(x => x.Mesa)
            .WithMany(x => x.Comandas)
            .HasForeignKey(x => x.MesaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UsuarioMesero)
            .WithMany()
            .HasForeignKey(x => x.UsuarioMeseroId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UsuarioCancelacion)
            .WithMany()
            .HasForeignKey(x => x.UsuarioCancelacionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ComensalConfiguration : IEntityTypeConfiguration<Comensal>
{
    public void Configure(EntityTypeBuilder<Comensal> builder)
    {
        builder.ToTable("Comensales");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => new { x.ComandaId, x.Numero }).IsUnique();
        builder.HasOne(x => x.Comanda)
            .WithMany(x => x.Comensales)
            .HasForeignKey(x => x.ComandaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ComandaDetalleConfiguration : IEntityTypeConfiguration<ComandaDetalle>
{
    public void Configure(EntityTypeBuilder<ComandaDetalle> builder)
    {
        builder.ToTable("ComandaDetalles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.NombreCategoriaHistorico).HasMaxLength(80).IsRequired();
        builder.Property(x => x.NombreProductoHistorico).HasMaxLength(120).IsRequired();
        builder.Property(x => x.PrecioUnitarioHistorico).HasPrecision(10, 2).IsRequired();
        builder.Property(x => x.Subtotal).HasPrecision(10, 2).IsRequired();
        builder.HasOne(x => x.Comensal)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.ComensalId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Producto)
            .WithMany()
            .HasForeignKey(x => x.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class EntregaParcialConfiguration : IEntityTypeConfiguration<EntregaParcial>
{
    public void Configure(EntityTypeBuilder<EntregaParcial> builder)
    {
        builder.ToTable("EntregasParciales");
        builder.HasKey(x => x.Id);
        builder.HasOne(x => x.ComandaDetalle)
            .WithMany(x => x.EntregasParciales)
            .HasForeignKey(x => x.ComandaDetalleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.UsuarioCocina)
            .WithMany()
            .HasForeignKey(x => x.UsuarioCocinaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UsuarioMeseroEntrega)
            .WithMany()
            .HasForeignKey(x => x.UsuarioMeseroEntregaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PagoComandaConfiguration : IEntityTypeConfiguration<PagoComanda>
{
    public void Configure(EntityTypeBuilder<PagoComanda> builder)
    {
        builder.ToTable("PagosComanda");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Importe).HasPrecision(10, 2).IsRequired();
        builder.HasOne(x => x.Comanda)
            .WithMany(x => x.Pagos)
            .HasForeignKey(x => x.ComandaId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UsuarioCobro)
            .WithMany()
            .HasForeignKey(x => x.UsuarioCobroId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class MermaComandaDetalleConfiguration : IEntityTypeConfiguration<MermaComandaDetalle>
{
    public void Configure(EntityTypeBuilder<MermaComandaDetalle> builder)
    {
        builder.ToTable("MermasComandaDetalle");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Motivo).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ImporteHistorico).HasPrecision(10, 2).IsRequired();
        builder.HasOne(x => x.ComandaDetalle)
            .WithMany(x => x.Mermas)
            .HasForeignKey(x => x.ComandaDetalleId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UsuarioRegistro)
            .WithMany()
            .HasForeignKey(x => x.UsuarioRegistroId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
