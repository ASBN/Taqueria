using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taqueria.Api.Controllers;
using Taqueria.Application.DTOs.Auth;
using Taqueria.Application.Interfaces.Services;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto, CancellationToken cancellationToken)
    {
        return Ok(await _service.LoginAsync(dto, cancellationToken));
    }

    [Authorize]
    [HttpPost("cambiar-password")]
    public async Task<ActionResult<AuthResponseDto>> CambiarPassword(
        CambiarPasswordDto dto,
        CancellationToken cancellationToken)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _service.CambiarPasswordAsync(id, dto, cancellationToken));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UsuarioSesionDto>> Me(CancellationToken cancellationToken)
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return Ok(await _service.ObtenerUsuarioActualAsync(id, cancellationToken));
    }
}
