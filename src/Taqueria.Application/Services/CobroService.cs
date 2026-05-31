namespace Taqueria.Application.Services;
using Taqueria.Application.DTOs.Cobros;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Application.Mappers;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Enums;
using Taqueria.Domain.Interfaces;

public sealed class CobroService : ICobroService
{
    private readonly IUnitOfWork _unitOfWork;

    public CobroService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<CuentaPendienteCobroDto>> ObtenerPendientesAsync(
        CancellationToken cancellationToken = default)
    {
        var pendientes = await _unitOfWork.Comandas.ObtenerPendientesCobroAsync(cancellationToken);
        return pendientes.Select(x => x.ToCuentaPendienteCobroDto()).ToList();
    }

    public async Task<CobroRegistradoDto> RegistrarCobroAsync(
        int comandaId,
        RegistrarCobroDto dto,
        int usuarioCobroId,
        CancellationToken cancellationToken = default)
    {
        var comanda = await ObtenerEntidadAsync(comandaId, cancellationToken);
        var pagos = PrepararPagos(dto, comanda.CalcularTotal());

        comanda.RegistrarCobro(pagos, usuarioCobroId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return (await ObtenerEntidadAsync(comandaId, cancellationToken)).ToCobroRegistradoDto();
    }

    private static IReadOnlyCollection<(MetodoPago MetodoPago, decimal Importe)> PrepararPagos(
        RegistrarCobroDto dto,
        decimal totalCuenta)
    {
        var pagosDto = dto.Pagos?.Where(x => x is not null).ToList() ?? new List<RegistrarPagoDto>();

        if (pagosDto.Count > 0 && !string.IsNullOrWhiteSpace(dto.MetodoPago))
        {
            throw new BusinessValidationException("Envíe un método único o una distribución de pagos, no ambos.");
        }

        if (pagosDto.Count == 0)
        {
            if (string.IsNullOrWhiteSpace(dto.MetodoPago))
            {
                throw new BusinessValidationException("Seleccione un método de pago o capture una distribución mixta.");
            }

            return new[] { (ConvertirMetodo(dto.MetodoPago), totalCuenta) };
        }

        if (pagosDto.Count > 3)
        {
            throw new BusinessValidationException("Sólo se admiten efectivo, tarjeta y transferencia una vez por cuenta.");
        }

        return pagosDto
            .Select(x => (ConvertirMetodo(x.MetodoPago), decimal.Round(x.Importe, 2, MidpointRounding.AwayFromZero)))
            .ToList();
    }

    private static MetodoPago ConvertirMetodo(string metodoPago)
    {
        if (!Enum.TryParse<MetodoPago>(metodoPago, ignoreCase: true, out var resultado)
            || !Enum.IsDefined(typeof(MetodoPago), resultado))
        {
            throw new BusinessValidationException("El método de pago debe ser Efectivo, Tarjeta o Transferencia.");
        }

        return resultado;
    }

    private async Task<Comanda> ObtenerEntidadAsync(int id, CancellationToken cancellationToken) =>
        await _unitOfWork.Comandas.ObtenerCompletaPorIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("La comanda solicitada no existe.");
}
