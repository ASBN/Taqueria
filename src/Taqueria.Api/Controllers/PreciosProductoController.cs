using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taqueria.Api.Controllers;
using Taqueria.Application.DTOs.PreciosProducto;
using Taqueria.Application.Interfaces.Services;

[ApiController]
[Authorize]
public sealed class PreciosProductoController : ControllerBase
{
    private readonly IPrecioProductoService _service;

    public PreciosProductoController(IPrecioProductoService service)
    {
        _service = service;
    }

    [HttpGet("api/productos/{productoId:int}/precios")]
    public async Task<ActionResult<IReadOnlyList<PrecioProductoDto>>> ObtenerPorProducto(int productoId, CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerPorProductoAsync(productoId, cancellationToken));

    [HttpGet("api/productos/{productoId:int}/precio-vigente")]
    public async Task<ActionResult<PrecioProductoDto?>> ObtenerVigente(int productoId, CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerVigenteAsync(productoId, cancellationToken));

    [Authorize(Roles = "Administrador")]
    [HttpPost("api/productos/{productoId:int}/precios")]
    public async Task<ActionResult<PrecioProductoDto>> Crear(int productoId, CrearPrecioProductoDto dto, CancellationToken cancellationToken) =>
        Ok(await _service.CrearAsync(productoId, dto, cancellationToken));

    [Authorize(Roles = "Administrador")]
    [HttpPut("api/precios-producto/{id:int}")]
    public async Task<ActionResult<PrecioProductoDto>> Actualizar(int id, ActualizarPrecioProductoDto dto, CancellationToken cancellationToken) =>
        Ok(await _service.ActualizarAsync(id, dto, cancellationToken));

    [Authorize(Roles = "Administrador")]
    [HttpDelete("api/precios-producto/{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
