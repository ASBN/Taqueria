using Microsoft.EntityFrameworkCore;

namespace Taqueria.Infrastructure.Persistence.Factories;

public sealed class SqlServerProviderFactory : IDatabaseProviderFactory
{
    public string ProviderName => "SqlServer";

    public void Configure(DbContextOptionsBuilder optionsBuilder, DatabaseOptions databaseOptions)
    {
        if (string.IsNullOrWhiteSpace(databaseOptions.ConnectionString))
        {
            throw new InvalidOperationException(
                "Database:ConnectionString es requerido cuando Database:Provider es SqlServer.");
        }

        optionsBuilder.UseSqlServer(
            databaseOptions.ConnectionString,
            sqlServer => sqlServer.MigrationsAssembly(typeof(TaqueriaDbContext).Assembly.FullName));
    }
}
