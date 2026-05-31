namespace Taqueria.Application.Services;
using Taqueria.Application.DTOs.Usuarios;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Interfaces.Security;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Application.Mappers;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;
using Taqueria.Domain.Interfaces;

public sealed class UsuarioService : IUsuarioService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashService _passwordHashService;

    public UsuarioService(IUnitOfWork unitOfWork, IPasswordHashService passwordHashService)
    {
        _unitOfWork = unitOfWork;
        _passwordHashService = passwordHashService;
    }

    public async Task<IReadOnlyList<UsuarioDto>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        var usuarios = await _unitOfWork.Usuarios.ObtenerTodosAsync(cancellationToken);
        return usuarios.Select(x => x.ToDto()).ToList();
    }

    public async Task<UsuarioDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        (await ObtenerEntidadAsync(id, cancellationToken)).ToDto();

    public async Task<UsuarioDto> CrearAsync(CrearUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        var nombreUsuario = NormalizarNombreUsuario(dto.NombreUsuario);
        if (await _unitOfWork.Usuarios.ExisteNombreUsuarioAsync(nombreUsuario, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"Ya existe el usuario '{nombreUsuario}'.");
        }

        AuthService.ValidarNuevoPassword(dto.PasswordTemporal, dto.ConfirmarPasswordTemporal);
        var usuario = new Usuario(
            dto.Nombre,
            nombreUsuario,
            _passwordHashService.Hash(dto.PasswordTemporal),
            ObtenerRol(dto.Rol),
            debeCambiarPassword: true);

        await _unitOfWork.Usuarios.AgregarAsync(usuario, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return usuario.ToDto();
    }

    public async Task<UsuarioDto> ActualizarAsync(
        int id,
        ActualizarUsuarioDto dto,
        int usuarioActualId,
        CancellationToken cancellationToken = default)
    {
        var usuario = await ObtenerEntidadAsync(id, cancellationToken);
        var nombreUsuario = NormalizarNombreUsuario(dto.NombreUsuario);
        var nuevoRol = ObtenerRol(dto.Rol);

        if (await _unitOfWork.Usuarios.ExisteNombreUsuarioAsync(nombreUsuario, id, cancellationToken))
        {
            throw new ConflictException($"Ya existe el usuario '{nombreUsuario}'.");
        }

        if (usuario.Id == usuarioActualId && !dto.Activo)
        {
            throw new BusinessValidationException("No puede desactivar su propia cuenta.");
        }

        await ValidarAdministradorNoQuedeSinAccesoAsync(usuario, nuevoRol, dto.Activo, cancellationToken);

        usuario.CambiarNombre(dto.Nombre);
        usuario.CambiarNombreUsuario(nombreUsuario);
        usuario.CambiarRol(nuevoRol);
        if (dto.Activo)
        {
            usuario.Activar();
        }
        else
        {
            usuario.Desactivar();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return usuario.ToDto();
    }

    public async Task<UsuarioDto> RestablecerPasswordAsync(
        int id,
        RestablecerPasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        var usuario = await ObtenerEntidadAsync(id, cancellationToken);
        AuthService.ValidarNuevoPassword(dto.PasswordTemporal, dto.ConfirmarPasswordTemporal);
        usuario.CambiarPassword(_passwordHashService.Hash(dto.PasswordTemporal), debeCambiarPassword: true);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return usuario.ToDto();
    }

    public async Task DesactivarAsync(int id, int usuarioActualId, CancellationToken cancellationToken = default)
    {
        var usuario = await ObtenerEntidadAsync(id, cancellationToken);
        if (usuario.Id == usuarioActualId)
        {
            throw new BusinessValidationException("No puede desactivar su propia cuenta.");
        }

        await ValidarAdministradorNoQuedeSinAccesoAsync(usuario, usuario.Rol, activo: false, cancellationToken);
        usuario.Desactivar();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Usuario> ObtenerEntidadAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.Usuarios.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("El usuario solicitado no existe.");

    private async Task ValidarAdministradorNoQuedeSinAccesoAsync(
        Usuario usuario,
        RolUsuario nuevoRol,
        bool activo,
        CancellationToken cancellationToken)
    {
        if (!usuario.Activo || usuario.Rol != RolUsuario.Administrador)
        {
            return;
        }

        var dejaraDeSerAdministradorActivo = !activo || nuevoRol != RolUsuario.Administrador;
        if (dejaraDeSerAdministradorActivo
            && await _unitOfWork.Usuarios.ContarActivosPorRolAsync(RolUsuario.Administrador, cancellationToken) <= 1)
        {
            throw new BusinessValidationException("Debe conservar al menos un administrador activo.");
        }
    }

    private static RolUsuario ObtenerRol(string rol)
    {
        if (!Enum.TryParse<RolUsuario>(rol, ignoreCase: true, out var resultado) || !Enum.IsDefined(typeof(RolUsuario), resultado))
        {
            throw new BusinessValidationException("Seleccione un rol válido.");
        }

        return resultado;
    }

    private static string NormalizarNombreUsuario(string nombreUsuario) =>
        !string.IsNullOrWhiteSpace(nombreUsuario)
            ? nombreUsuario.Trim().ToLowerInvariant()
            : throw new BusinessValidationException("El nombre de usuario es requerido.");
}
