using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taqueria.Api.Controllers;
using Taqueria.Application.DTOs.Comandas;
using Taqueria.Application.Interfaces.Services;

[ApiController]
[Authorize(Roles = "Administrador,Mesero")]
[Route("api/comandas")]
public sealed class ComandasController : ControllerBase
{
    private readonly IComandaService _service;

    public ComandasController(IComandaService service)
    {
        _service = service;
    }

    [HttpGet("activas")]
    public async Task<ActionResult<IReadOnlyList<ComandaDto>>> ObtenerActivas(CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerActivasAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ComandaDto>> ObtenerPorId(int id, CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerPorIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ComandaDto>> Crear(CrearComandaDto dto, CancellationToken cancellationToken)
    {
        var creada = await _service.CrearAsync(dto, ObtenerUsuarioId(), cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creada.Id }, creada);
    }

    [HttpPost("{id:int}/comensales")]
    public async Task<ActionResult<ComandaDto>> AgregarComensal(
        int id,
        AgregarComensalDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.AgregarComensalAsync(id, dto, ObtenerUsuarioId(), EsAdministrador(), cancellationToken));

    [HttpPost("{id:int}/comensales/{comensalId:int}/partidas")]
    public async Task<ActionResult<ComandaDto>> AgregarPartida(
        int id,
        int comensalId,
        AgregarPartidaDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.AgregarPartidaAsync(id, comensalId, dto, ObtenerUsuarioId(), EsAdministrador(), cancellationToken));

    [HttpDelete("{id:int}/comensales/{comensalId:int}/partidas/{detalleId:int}")]
    public async Task<ActionResult<ComandaDto>> QuitarPartida(
        int id,
        int comensalId,
        int detalleId,
        CancellationToken cancellationToken) =>
        Ok(await _service.QuitarPartidaAsync(id, comensalId, detalleId, ObtenerUsuarioId(), EsAdministrador(), cancellationToken));

    [HttpPost("{id:int}/enviar-cocina")]
    public async Task<ActionResult<ComandaDto>> EnviarACocina(int id, CancellationToken cancellationToken) =>
        Ok(await _service.EnviarACocinaAsync(id, ObtenerUsuarioId(), EsAdministrador(), cancellationToken));

    [HttpPost("{id:int}/entregas-parciales/{entregaParcialId:int}/confirmar")]
    public async Task<ActionResult<ComandaDto>> ConfirmarEntregaParcial(
        int id,
        int entregaParcialId,
        CancellationToken cancellationToken) =>
        Ok(await _service.ConfirmarEntregaParcialAsync(
            id,
            entregaParcialId,
            ObtenerUsuarioId(),
            EsAdministrador(),
            cancellationToken));

    [HttpPost("{id:int}/enviar-cobro")]
    public async Task<ActionResult<ComandaDto>> EnviarACobro(int id, CancellationToken cancellationToken) =>
        Ok(await _service.EnviarACobroAsync(id, ObtenerUsuarioId(), EsAdministrador(), cancellationToken));

    [HttpPost("{id:int}/cancelar")]
    public async Task<ActionResult<ComandaDto>> Cancelar(
        int id,
        CancelarComandaDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.CancelarAsync(id, dto, ObtenerUsuarioId(), EsAdministrador(), cancellationToken));

    private int ObtenerUsuarioId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool EsAdministrador() => User.IsInRole("Administrador");
}
