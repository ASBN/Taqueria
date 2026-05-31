using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Taqueria.Infrastructure.Persistence.Seed;
using Taqueria.Application.Interfaces.Security;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;

public sealed class DatabaseSeeder
{
    private readonly TaqueriaDbContext _context;
    private readonly IPasswordHashService _passwordHashService;
    private readonly SeedOptions _options;

    public DatabaseSeeder(
        TaqueriaDbContext context,
        IPasswordHashService passwordHashService,
        IOptions<SeedOptions> options)
    {
        _context = context;
        _passwordHashService = passwordHashService;
        _options = options.Value;
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await SeedUsuariosInicialesAsync(cancellationToken);

        if (!_options.LoadDemoCatalog)
        {
            return;
        }

        await SeedMesasDemoAsync(cancellationToken);
        await SeedCatalogoDemoAsync(cancellationToken);
    }

    private async Task SeedUsuariosInicialesAsync(CancellationToken cancellationToken)
    {
        var usuarios = new[]
        {
            new UsuarioInicial("Administrador", "admin", "admin123", RolUsuario.Administrador),
            new UsuarioInicial("Mesero Demo", "mesero", "mesero123", RolUsuario.Mesero),
            new UsuarioInicial("Cocina Demo", "cocina", "cocina123", RolUsuario.Cocina),
            new UsuarioInicial("Caja Demo", "caja", "caja123", RolUsuario.Caja)
        };

        foreach (var usuarioInicial in usuarios)
        {
            var usuarioExistente = await _context.Usuarios
                .FirstOrDefaultAsync(x => x.NombreUsuario == usuarioInicial.NombreUsuario, cancellationToken);

            if (usuarioExistente is not null)
            {
                // Sólo exigir cambio si la credencial demo nunca fue reemplazada.
                if (!usuarioExistente.DebeCambiarPassword
                    && _passwordHashService.Verify(usuarioInicial.Password, usuarioExistente.PasswordHash))
                {
                    usuarioExistente.CambiarPassword(usuarioExistente.PasswordHash, debeCambiarPassword: true);
                }

                continue;
            }

            await _context.Usuarios.AddAsync(
                new Usuario(
                    usuarioInicial.Nombre,
                    usuarioInicial.NombreUsuario,
                    _passwordHashService.Hash(usuarioInicial.Password),
                    usuarioInicial.Rol,
                    debeCambiarPassword: true),
                cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedMesasDemoAsync(CancellationToken cancellationToken)
    {
        if (await _context.Mesas.AnyAsync(cancellationToken))
        {
            return;
        }

        var mesas = Enumerable.Range(1, 10)
            .Select(numero => new Mesa(numero, numero <= 8 ? 4 : 6));

        await _context.Mesas.AddRangeAsync(mesas, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private async Task SeedCatalogoDemoAsync(CancellationToken cancellationToken)
    {
        if (await _context.CategoriasProducto.AnyAsync(cancellationToken))
        {
            return;
        }

        var categorias = new[]
        {
            new CategoriaProducto("Tacos", 1),
            new CategoriaProducto("Bebidas", 2),
            new CategoriaProducto("Quesadillas", 3),
            new CategoriaProducto("Postres", 4),
            new CategoriaProducto("Extras", 5)
        };

        await _context.CategoriasProducto.AddRangeAsync(categorias, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var productos = new[]
        {
            new Producto(categorias[0].Id, "Taco Pastor", 1),
            new Producto(categorias[0].Id, "Taco Suadero", 2),
            new Producto(categorias[0].Id, "Taco Bistec", 3),
            new Producto(categorias[1].Id, "Refresco", 1),
            new Producto(categorias[1].Id, "Agua Fresca", 2),
            new Producto(categorias[2].Id, "Quesadilla", 1)
        };

        await _context.Productos.AddRangeAsync(productos, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var inicio = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var precios = new[]
        {
            new PrecioProducto(productos[0].Id, 22m, inicio),
            new PrecioProducto(productos[1].Id, 24m, inicio),
            new PrecioProducto(productos[2].Id, 25m, inicio),
            new PrecioProducto(productos[3].Id, 28m, inicio),
            new PrecioProducto(productos[4].Id, 25m, inicio),
            new PrecioProducto(productos[5].Id, 45m, inicio)
        };

        await _context.PreciosProducto.AddRangeAsync(precios, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    private sealed record UsuarioInicial(string Nombre, string NombreUsuario, string Password, RolUsuario Rol);
}
