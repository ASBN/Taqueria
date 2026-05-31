using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taqueria.Api.Controllers;
using Taqueria.Application.DTOs.Usuarios;
using Taqueria.Application.Interfaces.Services;

[ApiController]
[Authorize(Roles = "Administrador")]
[Route("api/usuarios")]
public sealed class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UsuarioDto>>> ObtenerTodos(CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerTodosAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> ObtenerPorId(int id, CancellationToken cancellationToken) =>
        Ok(await _service.ObtenerPorIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<UsuarioDto>> Crear(CrearUsuarioDto dto, CancellationToken cancellationToken)
    {
        var creado = await _service.CrearAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<UsuarioDto>> Actualizar(
        int id,
        ActualizarUsuarioDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.ActualizarAsync(id, dto, ObtenerUsuarioId(), cancellationToken));

    [HttpPost("{id:int}/restablecer-password")]
    public async Task<ActionResult<UsuarioDto>> RestablecerPassword(
        int id,
        RestablecerPasswordDto dto,
        CancellationToken cancellationToken) =>
        Ok(await _service.RestablecerPasswordAsync(id, dto, cancellationToken));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Desactivar(int id, CancellationToken cancellationToken)
    {
        await _service.DesactivarAsync(id, ObtenerUsuarioId(), cancellationToken);
        return NoContent();
    }

    private int ObtenerUsuarioId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
