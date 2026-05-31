using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Taqueria.Infrastructure;
using Taqueria.Application.Interfaces.Reports;
using Taqueria.Application.Interfaces.Security;
using Taqueria.Domain.Interfaces;
using Taqueria.Domain.Interfaces.Repositories;
using Taqueria.Infrastructure.Persistence;
using Taqueria.Infrastructure.Persistence.Factories;
using Taqueria.Infrastructure.Persistence.Repositories;
using Taqueria.Infrastructure.Persistence.Seed;
using Taqueria.Infrastructure.Reports;
using Taqueria.Infrastructure.Security;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<SeedOptions>(configuration.GetSection(SeedOptions.SectionName));
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<ISQLiteConnectionStringResolver, SQLiteConnectionStringResolver>();
        services.AddSingleton<IDatabaseProviderFactory, SQLiteProviderFactory>();
        services.AddSingleton<IDatabaseProviderFactory, SqlServerProviderFactory>();

        var databaseOptions = configuration
            .GetSection(DatabaseOptions.SectionName)
            .Get<DatabaseOptions>() ?? new DatabaseOptions();

        services.AddDbContext<TaqueriaDbContext>((provider, optionsBuilder) =>
        {
            var factory = provider
                .GetServices<IDatabaseProviderFactory>()
                .FirstOrDefault(x => string.Equals(x.ProviderName, databaseOptions.Provider, StringComparison.OrdinalIgnoreCase))
                ?? throw new InvalidOperationException($"Proveedor de base de datos no soportado: {databaseOptions.Provider}.");

            factory.Configure(optionsBuilder, databaseOptions);
        });

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IMesaRepository, MesaRepository>();
        services.AddScoped<IComandaRepository, ComandaRepository>();
        services.AddScoped<IProductoRepository, ProductoRepository>();
        services.AddScoped<ICategoriaProductoRepository, CategoriaProductoRepository>();
        services.AddScoped<IPrecioProductoRepository, PrecioProductoRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddSingleton<IPasswordHashService, PasswordHashService>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IReporteExcelExporter, ClosedXmlReporteVentasExporter>();
        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
