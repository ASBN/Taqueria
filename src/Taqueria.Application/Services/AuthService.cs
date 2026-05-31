namespace Taqueria.Application.Services;
using Taqueria.Application.DTOs.Auth;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Interfaces.Security;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Application.Mappers;
using Taqueria.Domain.Interfaces;

public sealed class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHashService _passwordHashService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHashService passwordHashService,
        IJwtTokenService jwtTokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHashService = passwordHashService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreUsuario) || string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new BusinessValidationException("Usuario y contraseña son requeridos.");
        }

        var usuario = await _unitOfWork.Usuarios.ObtenerPorNombreUsuarioAsync(
            dto.NombreUsuario.Trim().ToLowerInvariant(),
            cancellationToken);

        if (usuario is null || !usuario.Activo || !_passwordHashService.Verify(dto.Password, usuario.PasswordHash))
        {
            throw new BusinessValidationException("Usuario o contraseña incorrectos.");
        }

        return CrearRespuesta(usuario);
    }

    public async Task<AuthResponseDto> CambiarPasswordAsync(
        int usuarioId,
        CambiarPasswordDto dto,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException("El usuario no existe.");

        if (!_passwordHashService.Verify(dto.PasswordActual, usuario.PasswordHash))
        {
            throw new BusinessValidationException("La contraseña actual es incorrecta.");
        }

        ValidarNuevoPassword(dto.NuevoPassword, dto.ConfirmarPassword, dto.PasswordActual);

        usuario.CambiarPassword(_passwordHashService.Hash(dto.NuevoPassword));
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return CrearRespuesta(usuario);
    }

    public async Task<UsuarioSesionDto> ObtenerUsuarioActualAsync(int usuarioId, CancellationToken cancellationToken = default)
    {
        var usuario = await _unitOfWork.Usuarios.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new NotFoundException("El usuario no existe.");

        return usuario.ToSesionDto();
    }

    private AuthResponseDto CrearRespuesta(Taqueria.Domain.Entities.Usuario usuario)
    {
        var token = _jwtTokenService.CrearToken(usuario);
        return new AuthResponseDto(token.Token, token.ExpiraUtc, usuario.ToSesionDto());
    }

    internal static void ValidarNuevoPassword(string nuevoPassword, string confirmarPassword, string? passwordAnterior = null)
    {
        if (string.IsNullOrWhiteSpace(nuevoPassword) || nuevoPassword.Length < 8)
        {
            throw new BusinessValidationException("La nueva contraseña debe tener al menos 8 caracteres.");
        }

        if (!nuevoPassword.Any(char.IsLetter) || !nuevoPassword.Any(char.IsDigit))
        {
            throw new BusinessValidationException("La nueva contraseña debe contener letras y números.");
        }

        if (!string.Equals(nuevoPassword, confirmarPassword, StringComparison.Ordinal))
        {
            throw new BusinessValidationException("La confirmación de contraseña no coincide.");
        }

        if (passwordAnterior is not null && string.Equals(nuevoPassword, passwordAnterior, StringComparison.Ordinal))
        {
            throw new BusinessValidationException("La nueva contraseña debe ser diferente de la actual.");
        }
    }
}
