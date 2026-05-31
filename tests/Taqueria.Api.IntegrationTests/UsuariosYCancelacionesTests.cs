using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Taqueria.Api.IntegrationTests;
using Taqueria.Application.DTOs.Auth;
using Taqueria.Application.DTOs.Cocina;
using Taqueria.Application.DTOs.Comandas;
using Taqueria.Application.DTOs.Mesas;
using Taqueria.Application.DTOs.Productos;
using Taqueria.Application.DTOs.Reportes;
using Taqueria.Application.DTOs.Usuarios;

public sealed class UsuariosYCancelacionesTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public UsuariosYCancelacionesTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UsuarioNuevo_DebeCambiarPasswordTemporalAntesDeOperar()
    {
        await _client.AutenticarConPasswordOperativaAsync("admin", "admin123");
        var alias = $"nuevo{Guid.NewGuid():N}"[..14];
        var crear = await _client.PostAsJsonAsync(
            "/api/usuarios",
            new CrearUsuarioDto("Nuevo Mesero", alias, "Temporal123", "Temporal123", "Mesero"));
        crear.EnsureSuccessStatusCode();
        var creado = (await crear.Content.ReadFromJsonAsync<UsuarioDto>())!;
        Assert.True(creado.DebeCambiarPassword);

        var login = await _client.PostAsJsonAsync("/api/auth/login", new LoginDto(alias, "Temporal123"));
        login.EnsureSuccessStatusCode();
        var auth = (await login.Content.ReadFromJsonAsync<AuthResponseDto>())!;
        Assert.True(auth.Usuario.DebeCambiarPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var bloqueado = await _client.GetAsync("/api/mesas");
        Assert.Equal(HttpStatusCode.Forbidden, bloqueado.StatusCode);

        var cambio = await _client.PostAsJsonAsync(
            "/api/auth/cambiar-password",
            new CambiarPasswordDto("Temporal123", "Operativa123", "Operativa123"));
        cambio.EnsureSuccessStatusCode();
        auth = (await cambio.Content.ReadFromJsonAsync<AuthResponseDto>())!;
        Assert.False(auth.Usuario.DebeCambiarPassword);
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Token);

        var permitido = await _client.GetAsync("/api/mesas");
        permitido.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CambioDeRol_TomaEfectoAunqueElUsuarioConserveSuTokenAnterior()
    {
        await _client.AutenticarConPasswordOperativaAsync("admin", "admin123");
        var alias = $"supervisor{Guid.NewGuid():N}"[..18];
        var crear = await _client.PostAsJsonAsync(
            "/api/usuarios",
            new CrearUsuarioDto("Supervisor Temporal", alias, "Temporal123", "Temporal123", "Administrador"));
        crear.EnsureSuccessStatusCode();
        var usuario = (await crear.Content.ReadFromJsonAsync<UsuarioDto>())!;

        var authSupervisor = await _client.AutenticarConPasswordOperativaAsync(alias, "Temporal123");
        var tokenAdministradorAnterior = authSupervisor.Token;

        await _client.AutenticarConPasswordOperativaAsync("admin", "admin123");
        var degradar = await _client.PutAsJsonAsync(
            $"/api/usuarios/{usuario.Id}",
            new ActualizarUsuarioDto(usuario.Nombre, usuario.NombreUsuario, "Mesero", Activo: true));
        degradar.EnsureSuccessStatusCode();

        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenAdministradorAnterior);
        var accesoAdministrativo = await _client.GetAsync("/api/usuarios");
        Assert.Equal(HttpStatusCode.Forbidden, accesoAdministrativo.StatusCode);
    }

    [Fact]
    public async Task Cancelacion_RegistraResponsable_YConviertePreparacionNoEntregadaEnMerma()
    {
        await _client.AutenticarConPasswordOperativaAsync("admin", "admin123");
        var mesa = (await _client.GetFromJsonAsync<IReadOnlyList<MesaDto>>("/api/mesas"))!.First(x => x.Activa);
        var producto = (await _client.GetFromJsonAsync<IReadOnlyList<ProductoDto>>("/api/productos"))!
            .First(x => x.Activo && x.PrecioVigente.HasValue);

        var abierta = await CrearComandaConPartidaAsync(mesa.Id, producto.Id);
        var cancelar = await _client.PostAsJsonAsync(
            $"/api/comandas/{abierta.Id}/cancelar",
            new CancelarComandaDto("El cliente cambió de mesa antes de preparar."));
        cancelar.EnsureSuccessStatusCode();
        var cancelada = (await cancelar.Content.ReadFromJsonAsync<ComandaDto>())!;
        Assert.Equal("Cancelada", cancelada.Estado);
        Assert.Equal("Administrador", cancelada.CanceladaPor);
        Assert.NotNull(cancelada.FechaHoraCancelacionUtc);

        var conPreparacion = await CrearComandaConPartidaAsync(mesa.Id, producto.Id);
        var enviar = await _client.PostAsJsonAsync($"/api/comandas/{conPreparacion.Id}/enviar-cocina", new { });
        enviar.EnsureSuccessStatusCode();
        conPreparacion = (await enviar.Content.ReadFromJsonAsync<ComandaDto>())!;
        var detalleId = conPreparacion.Comensales.Single().Detalles.Single().Id;
        var liberar = await _client.PostAsJsonAsync(
            $"/api/cocina/comandas/{conPreparacion.Id}/partidas/{detalleId}/entregas-parciales",
            new RegistrarEntregaParcialDto(1));
        liberar.EnsureSuccessStatusCode();

        var cancelacionConMerma = await _client.PostAsJsonAsync(
            $"/api/comandas/{conPreparacion.Id}/cancelar",
            new CancelarComandaDto("Cliente canceló después de preparar."));
        cancelacionConMerma.EnsureSuccessStatusCode();
        var canceladaConMerma = (await cancelacionConMerma.Content.ReadFromJsonAsync<ComandaDto>())!;
        Assert.Equal("Cancelada", canceladaConMerma.Estado);
        var merma = canceladaConMerma.Comensales.Single().Detalles.Single().Mermas.Single();
        Assert.Equal(1, merma.Cantidad);
        Assert.Equal(producto.PrecioVigente!.Value, merma.ImporteHistorico);
        Assert.Equal("Administrador", merma.RegistradaPor);

        var fechaLocal = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(-6));
        var reporteMermas = (await _client.GetFromJsonAsync<ReporteMermasDiarioDto>(
            $"/api/reportes/mermas-dia?fecha={fechaLocal:yyyy-MM-dd}&desfaseHorarioMinutos=360"))!;
        Assert.Contains(reporteMermas.Mermas, x => x.Comanda == canceladaConMerma.Folio && x.Cantidad == 1);

        var entregada = await CrearComandaConPartidaAsync(mesa.Id, producto.Id);
        enviar = await _client.PostAsJsonAsync($"/api/comandas/{entregada.Id}/enviar-cocina", new { });
        enviar.EnsureSuccessStatusCode();
        entregada = (await enviar.Content.ReadFromJsonAsync<ComandaDto>())!;
        detalleId = entregada.Comensales.Single().Detalles.Single().Id;
        liberar = await _client.PostAsJsonAsync(
            $"/api/cocina/comandas/{entregada.Id}/partidas/{detalleId}/entregas-parciales",
            new RegistrarEntregaParcialDto(1));
        liberar.EnsureSuccessStatusCode();
        var cocina = (await liberar.Content.ReadFromJsonAsync<ComandaCocinaDto>())!;
        var entregaId = cocina.Comensales.Single().Partidas.Single().EntregasParciales.Single().Id;
        var confirmar = await _client.PostAsJsonAsync(
            $"/api/comandas/{entregada.Id}/entregas-parciales/{entregaId}/confirmar",
            new { });
        confirmar.EnsureSuccessStatusCode();

        var cancelacionTrasEntrega = await _client.PostAsJsonAsync(
            $"/api/comandas/{entregada.Id}/cancelar",
            new CancelarComandaDto("No debe cancelar producto entregado."));
        Assert.Equal(HttpStatusCode.BadRequest, cancelacionTrasEntrega.StatusCode);
    }

    private async Task<ComandaDto> CrearComandaConPartidaAsync(int mesaId, int productoId)
    {
        var apertura = await _client.PostAsJsonAsync("/api/comandas", new CrearComandaDto(mesaId));
        apertura.EnsureSuccessStatusCode();
        var comanda = (await apertura.Content.ReadFromJsonAsync<ComandaDto>())!;
        var partida = await _client.PostAsJsonAsync(
            $"/api/comandas/{comanda.Id}/comensales/{comanda.Comensales.Single().Id}/partidas",
            new AgregarPartidaDto(productoId, 2));
        partida.EnsureSuccessStatusCode();
        return (await partida.Content.ReadFromJsonAsync<ComandaDto>())!;
    }
}
