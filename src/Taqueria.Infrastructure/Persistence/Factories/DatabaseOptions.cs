namespace Taqueria.Infrastructure.Persistence.Factories;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public string Provider { get; set; } = "SQLite";

    /// <summary>
    /// Conexión explícita. Se conserva para SQL Server, pruebas automatizadas o una ruta SQLite administrada manualmente.
    /// Para SQLite local, preferir la configuración SQLite y permitir que el sistema resuelva la carpeta de datos del usuario.
    /// </summary>
    public string? ConnectionString { get; set; }

    public SQLiteOptions SQLite { get; set; } = new();
}

public sealed class SQLiteOptions
{
    public string FileName { get; set; } = "taqueria.db";

    /// <summary>
    /// Auto: AzureHome al detectar App Service; LocalApplicationData en cualquier otro ambiente.
    /// Valores soportados: Auto, LocalApplicationData, AzureHome y Custom.
    /// </summary>
    public string StorageLocation { get; set; } = "Auto";

    public string? CustomDataDirectory { get; set; }
    public int DefaultTimeoutSeconds { get; set; } = 60;
    public bool EnableWriteAheadLogging { get; set; } = true;

    /// <summary>
    /// WAL es útil en disco local, pero no debe activarse por defecto sobre el almacenamiento persistente compartido de App Service.
    /// </summary>
    public bool EnableWriteAheadLoggingOnAzureHome { get; set; }
}
