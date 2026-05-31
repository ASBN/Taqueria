using Microsoft.Data.Sqlite;

namespace Taqueria.Infrastructure.Persistence.Factories;

public sealed class SQLiteConnectionStringResolver : ISQLiteConnectionStringResolver
{
    public string BuildConnectionString(DatabaseOptions options)
    {
        var sqliteOptions = options.SQLite ?? new SQLiteOptions();
        var connectionStringBuilder = !string.IsNullOrWhiteSpace(options.ConnectionString)
            ? new SqliteConnectionStringBuilder(options.ConnectionString)
            : CreateManagedConnectionStringBuilder(sqliteOptions);

        connectionStringBuilder.ForeignKeys = true;
        connectionStringBuilder.Pooling = true;
        connectionStringBuilder.DefaultTimeout = Math.Clamp(sqliteOptions.DefaultTimeoutSeconds, 1, 300);

        // Microsoft desaconseja usar Cache=Shared junto con WAL.
        if (ShouldEnableWriteAheadLogging(options) && connectionStringBuilder.Cache == SqliteCacheMode.Shared)
        {
            connectionStringBuilder.Cache = SqliteCacheMode.Default;
        }

        EnsureDataDirectoryExists(connectionStringBuilder.DataSource);
        return connectionStringBuilder.ToString();
    }

    public bool UsesAzureHomeStorage(DatabaseOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            var configuredDataSource = new SqliteConnectionStringBuilder(options.ConnectionString).DataSource;
            return IsInsideAzureHomeData(configuredDataSource);
        }

        var storageLocation = NormalizeStorageLocation(options.SQLite?.StorageLocation);
        return storageLocation == "AZUREHOME" || (storageLocation == "AUTO" && IsAzureAppService());
    }

    public bool ShouldEnableWriteAheadLogging(DatabaseOptions options)
    {
        var sqliteOptions = options.SQLite ?? new SQLiteOptions();
        return sqliteOptions.EnableWriteAheadLogging &&
            (!UsesAzureHomeStorage(options) || sqliteOptions.EnableWriteAheadLoggingOnAzureHome);
    }

    private static SqliteConnectionStringBuilder CreateManagedConnectionStringBuilder(SQLiteOptions options)
    {
        var directory = ResolveDataDirectory(options);
        Directory.CreateDirectory(directory);

        var fileName = string.IsNullOrWhiteSpace(options.FileName)
            ? "taqueria.db"
            : Path.GetFileName(options.FileName.Trim());

        return new SqliteConnectionStringBuilder
        {
            DataSource = Path.Combine(directory, fileName),
            Mode = SqliteOpenMode.ReadWriteCreate
        };
    }

    private static string ResolveDataDirectory(SQLiteOptions options)
    {
        var storageLocation = NormalizeStorageLocation(options.StorageLocation);
        return storageLocation switch
        {
            "AUTO" => IsAzureAppService() ? ResolveAzureHomeDirectory() : ResolveLocalApplicationDataDirectory(),
            "LOCALAPPLICATIONDATA" => ResolveLocalApplicationDataDirectory(),
            "AZUREHOME" => ResolveAzureHomeDirectory(),
            "CUSTOM" => ResolveCustomDirectory(options.CustomDataDirectory),
            _ => throw new InvalidOperationException(
                $"Database:SQLite:StorageLocation no soportado: {options.StorageLocation}. " +
                "Use Auto, LocalApplicationData, AzureHome o Custom.")
        };
    }

    private static string NormalizeStorageLocation(string? storageLocation) =>
        string.IsNullOrWhiteSpace(storageLocation)
            ? "AUTO"
            : storageLocation.Trim().Replace("_", string.Empty, StringComparison.Ordinal).ToUpperInvariant();

    private static string ResolveLocalApplicationDataDirectory()
    {
        var localApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localApplicationData))
        {
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            localApplicationData = Path.Combine(userProfile, ".local", "share");
        }

        return Path.Combine(localApplicationData, "Taqueria");
    }

    private static string ResolveAzureHomeDirectory()
    {
        var home = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrWhiteSpace(home))
        {
            throw new InvalidOperationException(
                "Se configuró almacenamiento AzureHome, pero la variable de entorno HOME no está disponible.");
        }

        return Path.Combine(home, "Data", "Taqueria");
    }

    private static string ResolveCustomDirectory(string? configuredDirectory)
    {
        if (string.IsNullOrWhiteSpace(configuredDirectory))
        {
            throw new InvalidOperationException(
                "Database:SQLite:CustomDataDirectory es requerido cuando StorageLocation es Custom.");
        }

        var expanded = Environment.ExpandEnvironmentVariables(configuredDirectory.Trim());
        return Path.GetFullPath(expanded);
    }

    private static bool IsInsideAzureHomeData(string? dataSource)
    {
        if (!IsAzureAppService() || string.IsNullOrWhiteSpace(dataSource) ||
            dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var home = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrWhiteSpace(home))
        {
            return false;
        }

        var azureDataRoot = Path.GetFullPath(Path.Combine(home, "Data")) + Path.DirectorySeparatorChar;
        var absoluteDataSource = Path.GetFullPath(dataSource);
        return absoluteDataSource.StartsWith(azureDataRoot, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsAzureAppService() =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME")) ||
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID"));

    private static void EnsureDataDirectoryExists(string dataSource)
    {
        if (string.IsNullOrWhiteSpace(dataSource) ||
            dataSource.Equals(":memory:", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fullPath = Path.GetFullPath(dataSource);
        var directory = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
