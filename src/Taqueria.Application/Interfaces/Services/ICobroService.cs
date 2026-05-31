namespace Taqueria.Application.Interfaces.Services;
using Taqueria.Application.DTOs.Cobros;

public interface ICobroService
{
    Task<IReadOnlyList<CuentaPendienteCobroDto>> ObtenerPendientesAsync(CancellationToken cancellationToken = default);
    Task<CobroRegistradoDto> RegistrarCobroAsync(
        int comandaId,
        RegistrarCobroDto dto,
        int usuarioCobroId,
        CancellationToken cancellationToken = default);
}
