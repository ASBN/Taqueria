using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taqueria.Api.Controllers;
using Taqueria.Application.DTOs.Productos;
using Taqueria.Application.Interfaces.Services;

[ApiController]
[Authorize]
[Route("api/productos")]
public sealed class ProductosController : ControllerBase
{
    private readonly IProductoService _service;

    public ProductosController(IProductoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ProductoDto>>> ObtenerTodos(CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerTodosAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductoDto>> ObtenerPorId(int id, CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerPorIdAsync(id, cancellationToken));

    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<ActionResult<ProductoDto>> Crear(CrearProductoDto dto, CancellationToken cancellationToken)
    {
        var creado = await _service.CrearAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [Authorize(Roles = "Administrador")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductoDto>> Actualizar(int id, ActualizarProductoDto dto, CancellationToken cancellationToken) =>
        Ok(await _service.ActualizarAsync(id, dto, cancellationToken));

    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
