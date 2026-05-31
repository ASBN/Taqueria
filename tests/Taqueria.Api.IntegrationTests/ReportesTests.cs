using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Taqueria.Api.IntegrationTests;
using Taqueria.Application.DTOs.Auth;
using Taqueria.Application.DTOs.Cobros;
using Taqueria.Application.DTOs.Cocina;
using Taqueria.Application.DTOs.Comandas;
using Taqueria.Application.DTOs.Mesas;
using Taqueria.Application.DTOs.Productos;
using Taqueria.Application.DTOs.Reportes;

public sealed class ReportesTests : IClassFixture<ApiFactory>
{
    private const int DesfaseMexicoCentral = 360;
    private readonly HttpClient _client;

    public ReportesTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CorteDiario_IncluyeVentaCobradaYCreaArchivoExcel()
    {
        await AutenticarAsync("admin", "admin123");
        var mesa = (await _client.GetFromJsonAsync<IReadOnlyList<MesaDto>>("/api/mesas"))!.First(x => x.Activa);
        var producto = (await _client.GetFromJsonAsync<IReadOnlyList<ProductoDto>>("/api/productos"))!
            .First(x => x.Activo && x.PrecioVigente.HasValue);

        var apertura = await _client.PostAsJsonAsync("/api/comandas", new CrearComandaDto(mesa.Id));
        apertura.EnsureSuccessStatusCode();
        var comanda = (await apertura.Content.ReadFromJsonAsync<ComandaDto>())!;
        var comensalId = comanda.Comensales.Single().Id;

        var partida = await _client.PostAsJsonAsync(
            $"/api/comandas/{comanda.Id}/comensales/{comensalId}/partidas",
            new AgregarPartidaDto(producto.Id, 2));
        partida.EnsureSuccessStatusCode();

        var envioCocina = await _client.PostAsJsonAsync($"/api/comandas/{comanda.Id}/enviar-cocina", new { });
        envioCocina.EnsureSuccessStatusCode();
        comanda = (await envioCocina.Content.ReadFromJsonAsync<ComandaDto>())!;
        var detalle = comanda.Comensales.Single().Detalles.Single();

        var liberacion = await _client.PostAsJsonAsync(
            $"/api/cocina/comandas/{comanda.Id}/partidas/{detalle.Id}/entregas-parciales",
            new RegistrarEntregaParcialDto(2));
        liberacion.EnsureSuccessStatusCode();
        var cocina = (await liberacion.Content.ReadFromJsonAsync<ComandaCocinaDto>())!;
        var entregaId = cocina.Comensales.Single().Partidas.Single().EntregasParciales.Single().Id;

        var entrega = await _client.PostAsJsonAsync(
            $"/api/comandas/{comanda.Id}/entregas-parciales/{entregaId}/confirmar",
            new { });
        entrega.EnsureSuccessStatusCode();
        var envioCaja = await _client.PostAsJsonAsync($"/api/comandas/{comanda.Id}/enviar-cobro", new { });
        envioCaja.EnsureSuccessStatusCode();
        var cobro = await _client.PostAsJsonAsync($"/api/cobros/comandas/{comanda.Id}", new RegistrarCobroDto("Tarjeta"));
        cobro.EnsureSuccessStatusCode();

        var fechaLocal = DateOnly.FromDateTime(DateTime.UtcNow.AddMinutes(-DesfaseMexicoCentral));
        var query = $"fecha={fechaLocal:yyyy-MM-dd}&desfaseHorarioMinutos={DesfaseMexicoCentral}";
        var reporte = (await _client.GetFromJsonAsync<ReporteVentasDiarioDto>($"/api/reportes/ventas-dia?{query}"))!;

        Assert.Equal(1, reporte.Resumen.NumeroVentas);
        Assert.Equal(2, reporte.Resumen.ProductosVendidos);
        Assert.Equal(producto.PrecioVigente!.Value * 2, reporte.Resumen.Total);
        Assert.Equal("Tarjeta", reporte.PorMetodoPago.Single().MetodoPago);
        Assert.Equal(producto.Nombre, reporte.PorProducto.Single().Producto);
        Assert.Equal(producto.CategoriaNombre, reporte.PorCategoria.Single().Categoria);

        var excel = await _client.GetAsync($"/api/reportes/ventas-dia/excel?{query}");
        excel.EnsureSuccessStatusCode();
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            excel.Content.Headers.ContentType?.MediaType);
        Assert.Contains($"Ventas_{fechaLocal:yyyy_MM_dd}.xlsx", excel.Content.Headers.ContentDisposition?.FileNameStar ?? excel.Content.Headers.ContentDisposition?.FileName);

        var bytes = await excel.Content.ReadAsByteArrayAsync();
        Assert.True(bytes.Length > 2);
        Assert.Equal((byte)'P', bytes[0]);
        Assert.Equal((byte)'K', bytes[1]);
    }

    private async Task AutenticarAsync(string usuario, string password)
    {
        await _client.AutenticarConPasswordOperativaAsync(usuario, password);
    }
}
