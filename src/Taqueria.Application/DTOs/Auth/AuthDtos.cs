namespace Taqueria.Application.DTOs.Auth;

public sealed record LoginDto(string NombreUsuario, string Password);

public sealed record CambiarPasswordDto(
    string PasswordActual,
    string NuevoPassword,
    string ConfirmarPassword);

public sealed record UsuarioSesionDto(
    int Id,
    string Nombre,
    string NombreUsuario,
    string Rol,
    bool Activo,
    bool DebeCambiarPassword);

public sealed record AuthResponseDto(
    string Token,
    DateTime ExpiraUtc,
    UsuarioSesionDto Usuario);
