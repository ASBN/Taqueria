using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taqueria.Infrastructure.Persistence.Configurations;
using Taqueria.Domain.Entities;

public sealed class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nombre).HasMaxLength(120).IsRequired();
        builder.Property(x => x.NombreUsuario).HasMaxLength(80).IsRequired();
        builder.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Rol).IsRequired();
        builder.Property(x => x.FechaCreacionUtc).IsRequired();
        builder.HasIndex(x => x.NombreUsuario).IsUnique();
    }
}
