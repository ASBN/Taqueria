namespace Taqueria.Application.Interfaces.Services;
using Taqueria.Application.DTOs.Cocina;

public interface ICocinaService
{
    Task<IReadOnlyList<ComandaCocinaDto>> ObtenerTableroAsync(
        bool incluirListas,
        CancellationToken cancellationToken = default);

    Task<ComandaCocinaDto> RegistrarEntregaParcialAsync(
        int comandaId,
        int detalleId,
        RegistrarEntregaParcialDto dto,
        int usuarioCocinaId,
        CancellationToken cancellationToken = default);
}
