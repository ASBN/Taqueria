using Microsoft.EntityFrameworkCore;

namespace Taqueria.Infrastructure.Persistence.Factories;

public sealed class SQLiteProviderFactory : IDatabaseProviderFactory
{
    private readonly ISQLiteConnectionStringResolver _connectionStringResolver;

    public SQLiteProviderFactory(ISQLiteConnectionStringResolver connectionStringResolver)
    {
        _connectionStringResolver = connectionStringResolver;
    }

    public string ProviderName => "SQLite";

    public void Configure(DbContextOptionsBuilder optionsBuilder, DatabaseOptions databaseOptions)
    {
        var connectionString = _connectionStringResolver.BuildConnectionString(databaseOptions);
        optionsBuilder.UseSqlite(
            connectionString,
            sqlite => sqlite.MigrationsAssembly(typeof(TaqueriaDbContext).Assembly.FullName));
    }
}
