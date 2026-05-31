using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Taqueria.Api.IntegrationTests;
using Taqueria.Application.DTOs.Auth;
using Taqueria.Application.DTOs.Cocina;
using Taqueria.Application.DTOs.Comandas;
using Taqueria.Application.DTOs.Mesas;
using Taqueria.Application.DTOs.Productos;

public sealed class CocinaTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CocinaTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FlujoCocina_RegistraEntregasParciales_YMarcaComandaLista()
    {
        await AutenticarComoAdministradorAsync();
        var comanda = await CrearComandaEnviadaACocinaAsync(cantidad: 50);
        var detalleId = comanda.Comensales.Single().Detalles.Single().Id;

        var tablero = await _client.GetFromJsonAsync<IReadOnlyList<ComandaCocinaDto>>(
            "/api/cocina/comandas?incluirListas=true");
        var orden = tablero!.Single(x => x.Id == comanda.Id);

        Assert.Equal("EnPreparacion", orden.Estado);
        Assert.Equal(50, orden.TotalPendientes);

        orden = await LiberarAsync(comanda.Id, detalleId, 20);
        Assert.Equal("ParcialmenteLista", orden.Estado);
        Assert.Equal(20, orden.TotalPreparados);
        Assert.Equal(30, orden.TotalPendientes);
        Assert.Single(orden.Comensales.Single().Partidas.Single().EntregasParciales);

        orden = await LiberarAsync(comanda.Id, detalleId, 15);
        Assert.Equal("ParcialmenteLista", orden.Estado);
        Assert.Equal(35, orden.TotalPreparados);
        Assert.Equal(15, orden.TotalPendientes);

        orden = await LiberarAsync(comanda.Id, detalleId, 15);
        Assert.Equal("Lista", orden.Estado);
        Assert.Equal(50, orden.TotalPreparados);
        Assert.Equal(0, orden.TotalPendientes);
        Assert.Equal(3, orden.Comensales.Single().Partidas.Single().EntregasParciales.Count);

        var liberacionExtra = await _client.PostAsJsonAsync(
            $"/api/cocina/comandas/{comanda.Id}/partidas/{detalleId}/entregas-parciales",
            new RegistrarEntregaParcialDto(1));

        Assert.Equal(HttpStatusCode.BadRequest, liberacionExtra.StatusCode);
    }

    private async Task<ComandaDto> CrearComandaEnviadaACocinaAsync(int cantidad)
    {
        var mesas = (await _client.GetFromJsonAsync<IReadOnlyList<MesaDto>>("/api/mesas"))!;
        var comandasActivas = (await _client.GetFromJsonAsync<IReadOnlyList<ComandaDto>>("/api/comandas/activas"))!;
        var ocupadas = comandasActivas.Select(x => x.MesaId).ToHashSet();
        var mesa = mesas.First(x => x.Activa && !ocupadas.Contains(x.Id));
        var producto = (await _client.GetFromJsonAsync<IReadOnlyList<ProductoDto>>("/api/productos"))!
            .First(x => x.Activo && x.PrecioVigente.HasValue);

        var apertura = await _client.PostAsJsonAsync("/api/comandas", new CrearComandaDto(mesa.Id));
        apertura.EnsureSuccessStatusCode();
        var comanda = (await apertura.Content.ReadFromJsonAsync<ComandaDto>())!;
        var comensalId = comanda.Comensales.Single().Id;

        var partida = await _client.PostAsJsonAsync(
            $"/api/comandas/{comanda.Id}/comensales/{comensalId}/partidas",
            new AgregarPartidaDto(producto.Id, cantidad));
        partida.EnsureSuccessStatusCode();

        var envio = await _client.PostAsJsonAsync($"/api/comandas/{comanda.Id}/enviar-cocina", new { });
        envio.EnsureSuccessStatusCode();
        return (await envio.Content.ReadFromJsonAsync<ComandaDto>())!;
    }

    private async Task<ComandaCocinaDto> LiberarAsync(int comandaId, int detalleId, int cantidad)
    {
        var liberacion = await _client.PostAsJsonAsync(
            $"/api/cocina/comandas/{comandaId}/partidas/{detalleId}/entregas-parciales",
            new RegistrarEntregaParcialDto(cantidad));
        liberacion.EnsureSuccessStatusCode();
        return (await liberacion.Content.ReadFromJsonAsync<ComandaCocinaDto>())!;
    }

    private async Task AutenticarComoAdministradorAsync()
    {
        await _client.AutenticarConPasswordOperativaAsync("admin", "admin123");
    }
}
