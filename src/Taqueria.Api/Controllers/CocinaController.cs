using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taqueria.Api.Controllers;
using Taqueria.Application.DTOs.Cocina;
using Taqueria.Application.Interfaces.Services;

[ApiController]
[Authorize(Roles = "Administrador,Cocina")]
[Route("api/cocina")]
public sealed class CocinaController : ControllerBase
{
    private readonly ICocinaService _service;

    public CocinaController(ICocinaService service)
    {
        _service = service;
    }

    [HttpGet("comandas")]
    public async Task<ActionResult<IReadOnlyList<ComandaCocinaDto>>> ObtenerTablero(
        [FromQuery] bool incluirListas = true,
        CancellationToken cancellationToken = default) =>
        Ok(await _service.ObtenerTableroAsync(incluirListas, cancellationToken));

    [HttpPost("comandas/{comandaId:int}/partidas/{detalleId:int}/entregas-parciales")]
    public async Task<ActionResult<ComandaCocinaDto>> RegistrarEntregaParcial(
        int comandaId,
        int detalleId,
        RegistrarEntregaParcialDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.RegistrarEntregaParcialAsync(
            comandaId,
            detalleId,
            dto,
            ObtenerUsuarioId(),
            cancellationToken));

    private int ObtenerUsuarioId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
