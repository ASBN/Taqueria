namespace Taqueria.Application.Mappers;
using Taqueria.Application.DTOs.Auth;
using Taqueria.Application.DTOs.Usuarios;
using Taqueria.Domain.Entities;

public static class UsuarioMapper
{
    public static UsuarioSesionDto ToSesionDto(this Usuario entidad) =>
        new(
            entidad.Id,
            entidad.Nombre,
            entidad.NombreUsuario,
            entidad.Rol.ToString(),
            entidad.Activo,
            entidad.DebeCambiarPassword);

    public static UsuarioDto ToDto(this Usuario entidad) =>
        new(
            entidad.Id,
            entidad.Nombre,
            entidad.NombreUsuario,
            entidad.Rol.ToString(),
            entidad.Activo,
            entidad.DebeCambiarPassword,
            entidad.FechaCreacionUtc);
}
