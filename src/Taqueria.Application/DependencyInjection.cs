using Microsoft.Extensions.DependencyInjection;

namespace Taqueria.Application;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Application.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IMesaService, MesaService>();
        services.AddScoped<ICategoriaProductoService, CategoriaProductoService>();
        services.AddScoped<IProductoService, ProductoService>();
        services.AddScoped<IPrecioProductoService, PrecioProductoService>();
        services.AddScoped<IComandaService, ComandaService>();
        services.AddScoped<ICocinaService, CocinaService>();
        services.AddScoped<ICobroService, CobroService>();
        services.AddScoped<IReporteService, ReporteService>();
        return services;
    }
}
