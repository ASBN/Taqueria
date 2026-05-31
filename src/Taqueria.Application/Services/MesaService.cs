namespace Taqueria.Application.Services;
using Taqueria.Application.DTOs.Mesas;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Application.Mappers;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces;

public sealed class MesaService : IMesaService
{
    private readonly IUnitOfWork _unitOfWork;

    public MesaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<MesaDto>> ObtenerTodasAsync(CancellationToken cancellationToken = default)
    {
        var mesas = await _unitOfWork.Mesas.ObtenerTodasAsync(cancellationToken);
        return mesas.Select(x => x.ToDto()).ToList();
    }

    public async Task<MesaDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var mesa = await ObtenerEntidadAsync(id, cancellationToken);
        return mesa.ToDto();
    }

    public async Task<MesaDto> CrearAsync(CrearMesaDto dto, CancellationToken cancellationToken = default)
    {
        if (await _unitOfWork.Mesas.ExisteNumeroAsync(dto.Numero, cancellationToken: cancellationToken))
        {
            throw new ConflictException($"Ya existe la mesa número {dto.Numero}.");
        }

        var mesa = new Mesa(dto.Numero, dto.Capacidad);
        await _unitOfWork.Mesas.AgregarAsync(mesa, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return mesa.ToDto();
    }

    public async Task<MesaDto> ActualizarAsync(int id, ActualizarMesaDto dto, CancellationToken cancellationToken = default)
    {
        var mesa = await ObtenerEntidadAsync(id, cancellationToken);

        if (await _unitOfWork.Mesas.ExisteNumeroAsync(dto.Numero, id, cancellationToken))
        {
            throw new ConflictException($"Ya existe la mesa número {dto.Numero}.");
        }

        mesa.Actualizar(dto.Numero, dto.Capacidad, dto.Activa);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return mesa.ToDto();
    }

    public async Task EliminarAsync(int id, CancellationToken cancellationToken = default)
    {
        var mesa = await ObtenerEntidadAsync(id, cancellationToken);

        if (await _unitOfWork.Mesas.TieneComandasAsync(id, cancellationToken))
        {
            mesa.Desactivar();
        }
        else
        {
            _unitOfWork.Mesas.Eliminar(mesa);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Mesa> ObtenerEntidadAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.Mesas.ObtenerPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("La mesa solicitada no existe.");
}
