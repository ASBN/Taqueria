using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Taqueria.Infrastructure.Persistence.Configurations;
using Taqueria.Domain.Entities;

public sealed class MesaConfiguration : IEntityTypeConfiguration<Mesa>
{
    public void Configure(EntityTypeBuilder<Mesa> builder)
    {
        builder.ToTable("Mesas");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Numero).IsUnique();
    }
}
