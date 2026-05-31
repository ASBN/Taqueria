namespace Taqueria.Application.Services;
using Taqueria.Application.DTOs.Comandas;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Application.Mappers;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces;

public sealed class ComandaService : IComandaService
{
    private readonly IUnitOfWork _unitOfWork;

    public ComandaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ComandaDto>> ObtenerActivasAsync(CancellationToken cancellationToken = default)
    {
        var comandas = await _unitOfWork.Comandas.ObtenerActivasAsync(cancellationToken);
        return comandas.Select(x => x.ToDto()).ToList();
    }

    public async Task<ComandaDto> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return (await ObtenerEntidadAsync(id, cancellationToken)).ToDto();
    }

    public async Task<ComandaDto> CrearAsync(
        CrearComandaDto dto,
        int usuarioMeseroId,
        CancellationToken cancellationToken = default)
    {
        var mesa = await _unitOfWork.Mesas.ObtenerPorIdAsync(dto.MesaId, cancellationToken)
            ?? throw new NotFoundException("La mesa solicitada no existe.");

        if (!mesa.Activa)
        {
            throw new BusinessValidationException("No se puede abrir una comanda en una mesa inactiva.");
        }

        if (await _unitOfWork.Comandas.ObtenerAbiertaPorMesaAsync(dto.MesaId, cancellationToken) is not null)
        {
            throw new ConflictException($"La mesa {mesa.Numero} ya tiene una comanda activa.");
        }

        var comanda = new Comanda(GenerarFolio(), dto.MesaId, usuarioMeseroId);
        await _unitOfWork.Comandas.AgregarAsync(comanda, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        comanda.AgregarComensal(1);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return (await ObtenerEntidadAsync(comanda.Id, cancellationToken)).ToDto();
    }

    public async Task<ComandaDto> AgregarComensalAsync(
        int comandaId,
        AgregarComensalDto dto,
        int usuarioId,
        bool esAdministrador,
        CancellationToken cancellationToken = default)
    {
        var comanda = await ObtenerEntidadAsync(comandaId, cancellationToken);
        ValidarPermiso(comanda, usuarioId, esAdministrador);

        comanda.AgregarComensal(dto.Numero);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await ObtenerEntidadAsync(comandaId, cancellationToken)).ToDto();
    }

    public async Task<ComandaDto> AgregarPartidaAsync(
        int comandaId,
        int comensalId,
        AgregarPartidaDto dto,
        int usuarioId,
        bool esAdministrador,
        CancellationToken cancellationToken = default)
    {
        var comanda = await ObtenerEntidadAsync(comandaId, cancellationToken);
        ValidarPermiso(comanda, usuarioId, esAdministrador);
        comanda.AsegurarCapturaAbierta();

        var comensal = comanda.Comensales.FirstOrDefault(x => x.Id == comensalId)
            ?? throw new NotFoundException("El comensal solicitado no existe en esta comanda.");

        var producto = await _unitOfWork.Productos.ObtenerConPreciosAsync(dto.ProductoId, cancellationToken)
            ?? throw new NotFoundException("El producto solicitado no existe.");

        if (!producto.Activo)
        {
            throw new BusinessValidationException("Seleccione un producto activo.");
        }

        var precioVigente = producto.ObtenerPrecioVigente(DateTime.UtcNow);
        comensal.AgregarPartida(producto, precioVigente, dto.Cantidad);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await ObtenerEntidadAsync(comandaId, cancellationToken)).ToDto();
    }

    public async Task<ComandaDto> QuitarPartidaAsync(
        int comandaId,
        int comensalId,
        int detalleId,
        int usuarioId,
        bool esAdministrador,
        CancellationToken cancellationToken = default)
    {
        var comanda = await ObtenerEntidadAsync(comandaId, cancellationToken);
        ValidarPermiso(comanda, usuarioId, esAdministrador);
        comanda.AsegurarCapturaAbierta();

        var comensal = comanda.Comensales.FirstOrDefault(x => x.Id == comensalId)
            ?? throw new NotFoundException("El comensal solicitado no existe en esta comanda.");

        comensal.QuitarPartida(detalleId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await ObtenerEntidadAsync(comandaId, cancellationToken)).ToDto();
    }

    public async Task<ComandaDto> EnviarACocinaAsync(
        int comandaId,
        int usuarioId,
        bool esAdministrador,
        CancellationToken cancellationToken = default)
    {
        var comanda = await ObtenerEntidadAsync(comandaId, cancellationToken);
        ValidarPermiso(comanda, usuarioId, esAdministrador);

        comanda.EnviarACocina();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await ObtenerEntidadAsync(comandaId, cancellationToken)).ToDto();
    }

    public async Task<ComandaDto> ConfirmarEntregaParcialAsync(
        int comandaId,
        int entregaParcialId,
        int usuarioId,
        bool esAdministrador,
        CancellationToken cancellationToken = default)
    {
        var comanda = await ObtenerEntidadAsync(comandaId, cancellationToken);
        ValidarPermiso(comanda, usuarioId, esAdministrador);

        comanda.ConfirmarEntregaParcial(entregaParcialId, usuarioId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await ObtenerEntidadAsync(comandaId, cancellationToken)).ToDto();
    }

    public async Task<ComandaDto> EnviarACobroAsync(
        int comandaId,
        int usuarioId,
        bool esAdministrador,
        CancellationToken cancellationToken = default)
    {
        var comanda = await ObtenerEntidadAsync(comandaId, cancellationToken);
        ValidarPermiso(comanda, usuarioId, esAdministrador);

        comanda.EnviarACobro();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await ObtenerEntidadAsync(comandaId, cancellationToken)).ToDto();
    }

    public async Task<ComandaDto> CancelarAsync(
        int comandaId,
        CancelarComandaDto dto,
        int usuarioId,
        bool esAdministrador,
        CancellationToken cancellationToken = default)
    {
        var comanda = await ObtenerEntidadAsync(comandaId, cancellationToken);
        ValidarPermiso(comanda, usuarioId, esAdministrador);

        comanda.Cancelar(dto.Motivo, usuarioId, esAdministrador);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await ObtenerEntidadAsync(comandaId, cancellationToken)).ToDto();
    }

    private async Task<Comanda> ObtenerEntidadAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.Comandas.ObtenerCompletaPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("La comanda solicitada no existe.");

    private static void ValidarPermiso(Comanda comanda, int usuarioId, bool esAdministrador)
    {
        if (!esAdministrador && comanda.UsuarioMeseroId != usuarioId)
        {
            throw new BusinessValidationException("Sólo el mesero responsable puede modificar esta comanda.");
        }
    }

    private static string GenerarFolio()
    {
        var sufijo = Guid.NewGuid().ToString("N")[..4].ToUpperInvariant();
        return $"TQ-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{sufijo}";
    }
}
