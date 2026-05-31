using System.Net;
using System.Net.Http.Json;

namespace Taqueria.Api.IntegrationTests;
using Taqueria.Application.DTOs.CategoriasProducto;

public sealed class AuthAndCatalogosTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public AuthAndCatalogosTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_AdminSeed_ObligaCambioPasswordYEntregaNuevoToken()
    {
        var auth = await _client.AutenticarConPasswordOperativaAsync("admin", "admin123");

        Assert.False(string.IsNullOrWhiteSpace(auth.Token));
        Assert.Equal("Administrador", auth.Usuario.Rol);
        Assert.False(auth.Usuario.DebeCambiarPassword);
    }

    [Fact]
    public async Task Categoria_RequiereToken_YPermiteCrudComoAdministrador()
    {
        _client.DefaultRequestHeaders.Authorization = null;
        var unauthorized = await _client.GetAsync("/api/categorias-producto");
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorized.StatusCode);

        await _client.AutenticarConPasswordOperativaAsync("admin", "admin123");
        var crear = await _client.PostAsJsonAsync("/api/categorias-producto", new CrearCategoriaProductoDto("Promociones", 20));
        crear.EnsureSuccessStatusCode();

        var categoria = await crear.Content.ReadFromJsonAsync<CategoriaProductoDto>();
        Assert.Equal("Promociones", categoria!.Nombre);
    }
}
