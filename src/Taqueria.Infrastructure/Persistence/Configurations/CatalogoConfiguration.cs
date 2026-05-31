using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taqueria.Infrastructure.Persistence.Configurations;
using Taqueria.Domain.Entities;

public sealed class CategoriaProductoConfiguration : IEntityTypeConfiguration<CategoriaProducto>
{
    public void Configure(EntityTypeBuilder<CategoriaProducto> builder)
    {
        builder.ToTable("CategoriasProducto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}

public sealed class ProductoConfiguration : IEntityTypeConfiguration<Producto>
{
    public void Configure(EntityTypeBuilder<Producto> builder)
    {
        builder.ToTable("Productos");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasOne(x => x.CategoriaProducto)
            .WithMany(x => x.Productos)
            .HasForeignKey(x => x.CategoriaProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PrecioProductoConfiguration : IEntityTypeConfiguration<PrecioProducto>
{
    public void Configure(EntityTypeBuilder<PrecioProducto> builder)
    {
        builder.ToTable("PreciosProducto");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Precio).HasPrecision(10, 2).IsRequired();
        builder.HasIndex(x => new { x.ProductoId, x.VigenteDesdeUtc }).IsUnique();
        builder.HasOne(x => x.Producto)
            .WithMany(x => x.Precios)
            .HasForeignKey(x => x.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
