namespace Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;
using Taqueria.Domain.Exceptions;

public sealed class Usuario
{
    private Usuario()
    {
    }

    public Usuario(string nombre, string nombreUsuario, string passwordHash, RolUsuario rol, bool debeCambiarPassword = false)
    {
        CambiarNombre(nombre);
        CambiarNombreUsuario(nombreUsuario);
        CambiarPassword(passwordHash, debeCambiarPassword);
        CambiarRol(rol);
        Activo = true;
        FechaCreacionUtc = DateTime.UtcNow;
    }

    public int Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string NombreUsuario { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public RolUsuario Rol { get; private set; }
    public bool Activo { get; private set; }
    public bool DebeCambiarPassword { get; private set; }
    public DateTime FechaCreacionUtc { get; private set; }

    public void CambiarNombre(string nombre)
    {
        Nombre = !string.IsNullOrWhiteSpace(nombre)
            ? nombre.Trim()
            : throw new DomainException("El nombre del usuario es requerido.");
    }

    public void CambiarNombreUsuario(string nombreUsuario)
    {
        NombreUsuario = !string.IsNullOrWhiteSpace(nombreUsuario)
            ? nombreUsuario.Trim().ToLowerInvariant()
            : throw new DomainException("El nombre de usuario es requerido.");
    }

    public void CambiarRol(RolUsuario rol)
    {
        Rol = Enum.IsDefined(typeof(RolUsuario), rol)
            ? rol
            : throw new DomainException("El rol seleccionado no es válido.");
    }

    public void CambiarPassword(string passwordHash, bool debeCambiarPassword = false)
    {
        PasswordHash = !string.IsNullOrWhiteSpace(passwordHash)
            ? passwordHash
            : throw new DomainException("El hash de contraseña es requerido.");
        DebeCambiarPassword = debeCambiarPassword;
    }

    public void Desactivar() => Activo = false;
    public void Activar() => Activo = true;
}
