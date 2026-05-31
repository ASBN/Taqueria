using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Taqueria.Infrastructure.Persistence;
using Taqueria.Infrastructure.Persistence.Factories;

public static class DatabaseInitializer
{
    public static async Task InitializeDatabaseAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TaqueriaDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TaqueriaDbContext>>();
        var databaseOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;

        if (context.Database.IsSqlite())
        {
            var resolver = scope.ServiceProvider.GetRequiredService<ISQLiteConnectionStringResolver>();
            var dataSource = context.Database.GetDbConnection().DataSource;
            logger.LogInformation("Base SQLite configurada en {DataSource}.", dataSource);

            if (resolver.ShouldEnableWriteAheadLogging(databaseOptions))
            {
                await context.Database.OpenConnectionAsync(cancellationToken);
                try
                {
                    await using var pragmaCommand = context.Database.GetDbConnection().CreateCommand();
                    pragmaCommand.CommandText = "PRAGMA journal_mode=WAL;";
                    await pragmaCommand.ExecuteScalarAsync(cancellationToken);
                    logger.LogInformation("SQLite opera con journal_mode=WAL para mejorar concurrencia local.");
                }
                finally
                {
                    await context.Database.CloseConnectionAsync();
                }
            }
            else if (resolver.UsesAzureHomeStorage(databaseOptions))
            {
                logger.LogWarning(
                    "SQLite está en almacenamiento HOME de Azure. WAL permanece deshabilitado porque el almacenamiento persistente de App Service es compartido. " +
                    "Use una sola instancia o migre a SQL Server/Azure SQL antes de escalar.");
            }
        }

        await context.Database.MigrateAsync(cancellationToken);

        var seeder = scope.ServiceProvider.GetRequiredService<Seed.DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
