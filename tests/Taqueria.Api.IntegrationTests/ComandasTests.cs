using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Taqueria.Api.IntegrationTests;
using Taqueria.Application.DTOs.Auth;
using Taqueria.Application.DTOs.Comandas;
using Taqueria.Application.DTOs.Mesas;
using Taqueria.Application.DTOs.Productos;

public sealed class ComandasTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public ComandasTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FlujoMesero_CopiaPrecioHistorico_YEnviaACocina()
    {
        await AutenticarComoAdministradorAsync();
        var mesa = (await _client.GetFromJsonAsync<IReadOnlyList<MesaDto>>("/api/mesas"))!.First(x => x.Activa);
        var producto = (await _client.GetFromJsonAsync<IReadOnlyList<ProductoDto>>("/api/productos"))!
            .First(x => x.Activo && x.PrecioVigente.HasValue);

        var apertura = await _client.PostAsJsonAsync("/api/comandas", new CrearComandaDto(mesa.Id));
        apertura.EnsureSuccessStatusCode();
        var comanda = (await apertura.Content.ReadFromJsonAsync<ComandaDto>())!;

        Assert.Equal("Abierta", comanda.Estado);
        Assert.Single(comanda.Comensales);

        var nuevaPersona = await _client.PostAsJsonAsync(
            $"/api/comandas/{comanda.Id}/comensales",
            new AgregarComensalDto(2));
        nuevaPersona.EnsureSuccessStatusCode();
        comanda = (await nuevaPersona.Content.ReadFromJsonAsync<ComandaDto>())!;
        Assert.Equal(2, comanda.Comensales.Count);

        var comensal = comanda.Comensales.First(x => x.Numero == 1);
        var partida = await _client.PostAsJsonAsync(
            $"/api/comandas/{comanda.Id}/comensales/{comensal.Id}/partidas",
            new AgregarPartidaDto(producto.Id, 3));
        partida.EnsureSuccessStatusCode();
        comanda = (await partida.Content.ReadFromJsonAsync<ComandaDto>())!;

        var detalle = comanda.Comensales.First(x => x.Numero == 1).Detalles.Single();
        Assert.Equal(producto.CategoriaProductoId, detalle.CategoriaProductoIdHistorico);
        Assert.Equal(producto.CategoriaNombre, detalle.NombreCategoriaHistorico);
        Assert.Equal(producto.Nombre, detalle.NombreProductoHistorico);
        Assert.Equal(producto.PrecioVigente, detalle.PrecioUnitarioHistorico);
        Assert.Equal(producto.PrecioVigente * 3, comanda.Total);

        var partidaEquivocada = await _client.PostAsJsonAsync(
            $"/api/comandas/{comanda.Id}/comensales/{comensal.Id}/partidas",
            new AgregarPartidaDto(producto.Id, 1));
        partidaEquivocada.EnsureSuccessStatusCode();
        comanda = (await partidaEquivocada.Content.ReadFromJsonAsync<ComandaDto>())!;
        var detalleARetirar = comanda.Comensales.First(x => x.Numero == 1).Detalles.Last();

        var retirada = await _client.DeleteAsync(
            $"/api/comandas/{comanda.Id}/comensales/{comensal.Id}/partidas/{detalleARetirar.Id}");
        retirada.EnsureSuccessStatusCode();
        comanda = (await retirada.Content.ReadFromJsonAsync<ComandaDto>())!;
        Assert.Single(comanda.Comensales.First(x => x.Numero == 1).Detalles);
        Assert.Equal(producto.PrecioVigente * 3, comanda.Total);

        var envio = await _client.PostAsJsonAsync($"/api/comandas/{comanda.Id}/enviar-cocina", new { });
        envio.EnsureSuccessStatusCode();
        comanda = (await envio.Content.ReadFromJsonAsync<ComandaDto>())!;

        Assert.Equal("EnPreparacion", comanda.Estado);
        Assert.NotNull(comanda.FechaHoraEnvioCocinaUtc);
    }

    [Fact]
    public async Task Apertura_NoPermiteDosComandasActivasEnLaMismaMesa()
    {
        await AutenticarComoAdministradorAsync();
        var mesa = (await _client.GetFromJsonAsync<IReadOnlyList<MesaDto>>("/api/mesas"))!.Last(x => x.Activa);

        var primera = await _client.PostAsJsonAsync("/api/comandas", new CrearComandaDto(mesa.Id));
        primera.EnsureSuccessStatusCode();

        var segunda = await _client.PostAsJsonAsync("/api/comandas", new CrearComandaDto(mesa.Id));

        Assert.Equal(HttpStatusCode.Conflict, segunda.StatusCode);
    }

    private async Task AutenticarComoAdministradorAsync()
    {
        await _client.AutenticarConPasswordOperativaAsync("admin", "admin123");
    }
}
