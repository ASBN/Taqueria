using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taqueria.Api.Controllers;
using Taqueria.Application.DTOs.Mesas;
using Taqueria.Application.Interfaces.Services;

[ApiController]
[Authorize]
[Route("api/mesas")]
public sealed class MesasController : ControllerBase
{
    private readonly IMesaService _service;

    public MesasController(IMesaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<MesaDto>>> ObtenerTodas(CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerTodasAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MesaDto>> ObtenerPorId(int id, CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerPorIdAsync(id, cancellationToken));

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<ActionResult<MesaDto>> Crear(CrearMesaDto dto, CancellationToken cancellationToken)
    {
        var creado = await _service.CrearAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MesaDto>> Actualizar(int id, ActualizarMesaDto dto, CancellationToken cancellationToken) =>
        Ok(await _service.ActualizarAsync(id, dto, cancellationToken));

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
