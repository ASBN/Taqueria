using System.Security.Claims;
using System.Text.Json;

namespace Taqueria.Api.Middleware;
using Taqueria.Application.Interfaces.Services;

public sealed class UsuarioOperativoMiddleware
{
    private readonly RequestDelegate _next;

    public UsuarioOperativoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var usuarioIdClaim = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
        {
            await ResponderAsync(context, StatusCodes.Status401Unauthorized, "La sesión no contiene un usuario válido.", "INVALID_SESSION");
            return;
        }

        var usuario = await authService.ObtenerUsuarioActualAsync(usuarioId, context.RequestAborted);
        if (!usuario.Activo)
        {
            await ResponderAsync(context, StatusCodes.Status401Unauthorized, "El usuario está desactivado.", "USER_DISABLED");
            return;
        }

        if (usuario.DebeCambiarPassword && !EsRutaPermitida(context.Request.Path))
        {
            await ResponderAsync(
                context,
                StatusCodes.Status403Forbidden,
                "Debe cambiar su contraseña temporal antes de continuar.",
                "PASSWORD_CHANGE_REQUIRED");
            return;
        }

        SincronizarRolVigente(context, usuario.Rol);
        await _next(context);
    }

    private static void SincronizarRolVigente(HttpContext context, string rolVigente)
    {
        if (context.User.Identity is not ClaimsIdentity identity)
        {
            return;
        }

        foreach (var claim in identity.FindAll(ClaimTypes.Role).ToList())
        {
            identity.RemoveClaim(claim);
        }

        identity.AddClaim(new Claim(ClaimTypes.Role, rolVigente));
    }

    private static Task ResponderAsync(HttpContext context, int statusCode, string mensaje, string codigo)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(new { mensaje, codigo }));
    }

    private static bool EsRutaPermitida(PathString path) =>
        path.StartsWithSegments("/api/auth/cambiar-password")
        || path.StartsWithSegments("/api/auth/me")
        || path.StartsWithSegments("/swagger");
}
