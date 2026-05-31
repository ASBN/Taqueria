using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Taqueria.Api.IntegrationTests;
using Taqueria.Application.DTOs.Auth;
using Taqueria.Application.DTOs.Cobros;
using Taqueria.Application.DTOs.Cocina;
using Taqueria.Application.DTOs.Comandas;
using Taqueria.Application.DTOs.Mesas;
using Taqueria.Application.DTOs.Productos;

public sealed class CobrosTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CobrosTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task FlujoCompleto_SeparaMeseroCocinaYCaja_YLiberaMesaAlCobrar()
    {
        await AutenticarAsync("mesero", "mesero123");
        var mesa = await ObtenerMesaLibreAsync();
        var producto = (await _client.GetFromJsonAsync<IReadOnlyList<ProductoDto>>("/api/productos"))!
            .First(x => x.Activo && x.PrecioVigente.HasValue);

        var apertura = await _client.PostAsJsonAsync("/api/comandas", new CrearComandaDto(mesa.Id));
        apertura.EnsureSuccessStatusCode();
        var comanda = (await apertura.Content.ReadFromJsonAsync<ComandaDto>())!;
        var comensalId = comanda.Comensales.Single().Id;

        var partida = await _client.PostAsJsonAsync(
            $"/api/comandas/{comanda.Id}/comensales/{comensalId}/partidas",
            new AgregarPartidaDto(producto.Id, 4));
        partida.EnsureSuccessStatusCode();

        var envioCocina = await _client.PostAsJsonAsync($"/api/comandas/{comanda.Id}/enviar-cocina", new { });
        envioCocina.EnsureSuccessStatusCode();
        comanda = (await envioCocina.Content.ReadFromJsonAsync<ComandaDto>())!;
        var detalleId = comanda.Comensales.Single().Detalles.Single().Id;
        var totalEsperado = producto.PrecioVigente!.Value * 4;

        await AutenticarAsync("cocina", "cocina123");
        var liberacion = await _client.PostAsJsonAsync(
            $"/api/cocina/comandas/{comanda.Id}/partidas/{detalleId}/entregas-parciales",
            new RegistrarEntregaParcialDto(4));
        liberacion.EnsureSuccessStatusCode();
        var cocina = (await liberacion.Content.ReadFromJsonAsync<ComandaCocinaDto>())!;
        Assert.Equal("Lista", cocina.Estado);
        var entregaId = cocina.Comensales.Single().Partidas.Single().EntregasParciales.Single().Id;

        await AutenticarAsync("mesero", "mesero123");
        var entrega = await _client.PostAsJsonAsync(
            $"/api/comandas/{comanda.Id}/entregas-parciales/{entregaId}/confirmar",
            new { });
        entrega.EnsureSuccessStatusCode();
        comanda = (await entrega.Content.ReadFromJsonAsync<ComandaDto>())!;
        Assert.Equal("Entregada", comanda.Estado);
        Assert.Equal(4, comanda.Comensales.Single().Detalles.Single().CantidadEntregada);

        var envioCaja = await _client.PostAsJsonAsync($"/api/comandas/{comanda.Id}/enviar-cobro", new { });
        envioCaja.EnsureSuccessStatusCode();
        comanda = (await envioCaja.Content.ReadFromJsonAsync<ComandaDto>())!;
        Assert.Equal("PendienteCobro", comanda.Estado);

        await AutenticarAsync("caja", "caja123");
        var pendientes = (await _client.GetFromJsonAsync<IReadOnlyList<CuentaPendienteCobroDto>>("/api/cobros/pendientes"))!;
        var cuenta = pendientes.Single(x => x.ComandaId == comanda.Id);
        Assert.Equal(totalEsperado, cuenta.Total);

        var intentoCapturaCaja = await _client.PostAsJsonAsync("/api/comandas", new CrearComandaDto(mesa.Id));
        Assert.Equal(HttpStatusCode.Forbidden, intentoCapturaCaja.StatusCode);

        var efectivo = decimal.Round(totalEsperado / 2m, 2, MidpointRounding.AwayFromZero);
        var tarjeta = totalEsperado - efectivo;
        var cobro = await _client.PostAsJsonAsync(
            $"/api/cobros/comandas/{comanda.Id}",
            new RegistrarCobroDto(Pagos: new List<RegistrarPagoDto>
            {
                new("Efectivo", efectivo),
                new("Tarjeta", tarjeta)
            }));
        cobro.EnsureSuccessStatusCode();
        var recibido = (await cobro.Content.ReadFromJsonAsync<CobroRegistradoDto>())!;
        Assert.Equal("Cobrada", recibido.Estado);
        Assert.Equal(totalEsperado, recibido.TotalCobrado);
        Assert.Equal(2, recibido.Pagos.Count);
        Assert.Equal(totalEsperado, recibido.Pagos.Sum(x => x.Importe));
        Assert.Contains(recibido.Pagos, x => x.MetodoPago == "Efectivo" && x.Importe == efectivo);
        Assert.Contains(recibido.Pagos, x => x.MetodoPago == "Tarjeta" && x.Importe == tarjeta);

        await AutenticarAsync("mesero", "mesero123");
        var reapertura = await _client.PostAsJsonAsync("/api/comandas", new CrearComandaDto(mesa.Id));
        reapertura.EnsureSuccessStatusCode();
    }

    private async Task<MesaDto> ObtenerMesaLibreAsync()
    {
        var mesas = (await _client.GetFromJsonAsync<IReadOnlyList<MesaDto>>("/api/mesas"))!;
        var comandas = (await _client.GetFromJsonAsync<IReadOnlyList<ComandaDto>>("/api/comandas/activas"))!;
        var ocupadas = comandas.Select(x => x.MesaId).ToHashSet();
        return mesas.First(x => x.Activa && !ocupadas.Contains(x.Id));
    }

    private async Task AutenticarAsync(string usuario, string password)
    {
        await _client.AutenticarConPasswordOperativaAsync(usuario, password);
    }
}
