namespace Taqueria.Application.DTOs.Usuarios;

public sealed record CrearUsuarioDto(
    string Nombre,
    string NombreUsuario,
    string PasswordTemporal,
    string ConfirmarPasswordTemporal,
    string Rol);

public sealed record ActualizarUsuarioDto(
    string Nombre,
    string NombreUsuario,
    string Rol,
    bool Activo);

public sealed record RestablecerPasswordDto(
    string PasswordTemporal,
    string ConfirmarPasswordTemporal);

public sealed record UsuarioDto(
    int Id,
    string Nombre,
    string NombreUsuario,
    string Rol,
    bool Activo,
    bool DebeCambiarPassword,
    DateTime FechaCreacionUtc);
