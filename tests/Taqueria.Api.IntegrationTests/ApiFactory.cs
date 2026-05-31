using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Taqueria.Api.IntegrationTests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"taqueria-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:Provider"] = "SQLite",
                ["Database:ConnectionString"] = $"Data Source={_databasePath}",
                ["Database:SQLite:EnableWriteAheadLogging"] = "false",
                ["Seed:LoadDemoCatalog"] = "true",
                ["Jwt:Issuer"] = "Taqueria.Api.Tests",
                ["Jwt:Audience"] = "Taqueria.Web.Tests",
                ["Jwt:Key"] = "LLAVE-SEGURA-PARA-PRUEBAS-INTEGRACION-TAQUERIA-000001",
                ["Jwt:ExpirationMinutes"] = "60"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        foreach (var suffix in new[] { string.Empty, "-shm", "-wal", "-journal" })
        {
            var file = _databasePath + suffix;
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }
    }
}
