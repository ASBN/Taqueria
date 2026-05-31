using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taqueria.Api.Controllers;
using Taqueria.Application.DTOs.Cobros;
using Taqueria.Application.Interfaces.Services;

[ApiController]
[Authorize(Roles = "Administrador,Caja")]
[Route("api/cobros")]
public sealed class CobrosController : ControllerBase
{
    private readonly ICobroService _service;

    public CobrosController(ICobroService service)
    {
        _service = service;
    }

    [HttpGet("pendientes")]
    public async Task<ActionResult<IReadOnlyList<CuentaPendienteCobroDto>>> ObtenerPendientes(
        CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerPendientesAsync(cancellationToken));

    [HttpPost("comandas/{comandaId:int}")]
    public async Task<ActionResult<CobroRegistradoDto>> RegistrarCobro(
        int comandaId,
        RegistrarCobroDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.RegistrarCobroAsync(comandaId, dto, ObtenerUsuarioId(), cancellationToken));

    private int ObtenerUsuarioId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
