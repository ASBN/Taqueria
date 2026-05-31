using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taqueria.Api.Controllers;
using Taqueria.Application.DTOs.CategoriasProducto;
using Taqueria.Application.Interfaces.Services;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/categorias-producto")]
public sealed class CategoriasProductoController : ControllerBase
{
    private readonly ICategoriaProductoService _service;

    public CategoriasProductoController(ICategoriaProductoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoriaProductoDto>>> ObtenerTodas(CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerTodasAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaProductoDto>> ObtenerPorId(int id, CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerPorIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CategoriaProductoDto>> Crear(CrearCategoriaProductoDto dto, CancellationToken cancellationToken)
    {
        var creado = await _service.CrearAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CategoriaProductoDto>> Actualizar(int id, ActualizarCategoriaProductoDto dto, CancellationToken cancellationToken) =>
        Ok(await _service.ActualizarAsync(id, dto, cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id, CancellationToken cancellationToken)
    {
        await _service.EliminarAsync(id, cancellationToken);
        return NoContent();
    }
}
