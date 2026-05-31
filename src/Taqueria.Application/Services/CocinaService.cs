namespace Taqueria.Application.Services;
using Taqueria.Application.DTOs.Cocina;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Application.Mappers;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces;

public sealed class CocinaService : ICocinaService
{
    private readonly IUnitOfWork _unitOfWork;

    public CocinaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ComandaCocinaDto>> ObtenerTableroAsync(
        bool incluirListas,
        CancellationToken cancellationToken = default)
    {
        var comandas = await _unitOfWork.Comandas.ObtenerActivasParaCocinaAsync(incluirListas, cancellationToken);
        return comandas.Select(x => x.ToCocinaDto()).ToList();
    }

    public async Task<ComandaCocinaDto> RegistrarEntregaParcialAsync(
        int comandaId,
        int detalleId,
        RegistrarEntregaParcialDto dto,
        int usuarioCocinaId,
        CancellationToken cancellationToken = default)
    {
        var comanda = await ObtenerEntidadAsync(comandaId, cancellationToken);
        comanda.LiberarEntregaParcial(detalleId, dto.Cantidad, usuarioCocinaId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await ObtenerEntidadAsync(comandaId, cancellationToken)).ToCocinaDto();
    }

    private async Task<Comanda> ObtenerEntidadAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.Comandas.ObtenerCompletaPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("La comanda solicitada no existe.");
}
