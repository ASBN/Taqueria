namespace Taqueria.Infrastructure.Persistence.Factories;

public interface ISQLiteConnectionStringResolver
{
    string BuildConnectionString(DatabaseOptions options);
    bool UsesAzureHomeStorage(DatabaseOptions options);
    bool ShouldEnableWriteAheadLogging(DatabaseOptions options);
}
