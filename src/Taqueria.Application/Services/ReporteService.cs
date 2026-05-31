namespace Taqueria.Application.Services;
using Taqueria.Application.DTOs.Reportes;
using Taqueria.Application.Exceptions;
using Taqueria.Application.Interfaces.Reports;
using Taqueria.Application.Interfaces.Services;
using Taqueria.Domain.Entities;
using Taqueria.Domain.Interfaces;

public sealed class ReporteService : IReporteService
{
    private const int DesfaseMinimo = -840;
    private const int DesfaseMaximo = 840;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReporteExcelExporter _excelExporter;

    public ReporteService(IUnitOfWork unitOfWork, IReporteExcelExporter excelExporter)
    {
        _unitOfWork = unitOfWork;
        _excelExporter = excelExporter;
    }

    public async Task<ReporteVentasDiarioDto> ObtenerVentasDiaAsync(
        DateOnly fecha,
        int desfaseHorarioMinutos,
        CancellationToken cancellationToken = default)
    {
        ValidarDesfaseHorario(desfaseHorarioMinutos);

        var desdeUtc = ConvertirInicioDiaAUtc(fecha, desfaseHorarioMinutos);
        var hastaUtcExclusiva = ConvertirInicioDiaAUtc(fecha.AddDays(1), desfaseHorarioMinutos);
        var comandas = await _unitOfWork.Comandas.ObtenerCobradasPorFechaCobroAsync(
            desdeUtc,
            hastaUtcExclusiva,
            cancellationToken);

        return ConstruirReporteVentas(fecha, desfaseHorarioMinutos, desdeUtc, hastaUtcExclusiva, comandas);
    }

    public async Task<ReporteMermasDiarioDto> ObtenerMermasDiaAsync(
        DateOnly fecha,
        int desfaseHorarioMinutos,
        CancellationToken cancellationToken = default)
    {
        ValidarDesfaseHorario(desfaseHorarioMinutos);

        var desdeUtc = ConvertirInicioDiaAUtc(fecha, desfaseHorarioMinutos);
        var hastaUtcExclusiva = ConvertirInicioDiaAUtc(fecha.AddDays(1), desfaseHorarioMinutos);
        var comandas = await _unitOfWork.Comandas.ObtenerCanceladasConMermasPorFechaAsync(
            desdeUtc,
            hastaUtcExclusiva,
            cancellationToken);

        var mermas = comandas
            .SelectMany(comanda => comanda.Comensales.SelectMany(comensal => comensal.Detalles.SelectMany(detalle =>
                detalle.Mermas.Select(merma => new MermaDiaDto(
                    merma.FechaHoraRegistroUtc,
                    comanda.Mesa?.Numero ?? 0,
                    comanda.Folio,
                    detalle.NombreProductoHistorico,
                    detalle.NombreCategoriaHistorico,
                    merma.Cantidad,
                    merma.ImporteHistorico,
                    merma.Motivo,
                    merma.UsuarioRegistro?.Nombre ?? comanda.UsuarioCancelacion?.Nombre ?? string.Empty)))))
            .OrderBy(x => x.FechaHoraRegistroUtc)
            .ToList();

        return new ReporteMermasDiarioDto(
            fecha,
            desfaseHorarioMinutos,
            desdeUtc,
            hastaUtcExclusiva,
            new ResumenMermasDiaDto(
                comandas.Count(x => x.Comensales.SelectMany(c => c.Detalles).SelectMany(d => d.Mermas).Any()),
                mermas.Sum(x => x.Cantidad),
                mermas.Sum(x => x.ImporteHistorico)),
            mermas);
    }

    public async Task<ArchivoReporteDto> ExportarVentasDiaExcelAsync(
        DateOnly fecha,
        int desfaseHorarioMinutos,
        CancellationToken cancellationToken = default)
    {
        var ventas = await ObtenerVentasDiaAsync(fecha, desfaseHorarioMinutos, cancellationToken);
        var mermas = await ObtenerMermasDiaAsync(fecha, desfaseHorarioMinutos, cancellationToken);
        return _excelExporter.ExportarCorteDiario(ventas, mermas);
    }

    private static ReporteVentasDiarioDto ConstruirReporteVentas(
        DateOnly fecha,
        int desfaseHorarioMinutos,
        DateTime desdeUtc,
        DateTime hastaUtcExclusiva,
        IReadOnlyList<Comanda> comandas)
    {
        var ventas = comandas
            .OrderBy(x => x.FechaHoraCobroUtc)
            .Select(comanda => new VentaDiaDto(
                comanda.FechaHoraCobroUtc!.Value,
                comanda.Mesa?.Numero ?? 0,
                comanda.Folio,
                comanda.Pagos.Count == 1 ? comanda.Pagos.Single().MetodoPago.ToString() : "Mixto",
                comanda.TotalCobrado ?? comanda.Pagos.Sum(x => x.Importe),
                comanda.UsuarioMesero?.Nombre ?? string.Empty,
                comanda.FechaHoraAperturaUtc,
                comanda.FechaHoraCobroUtc!.Value))
            .ToList();

        var detalles = comandas
            .SelectMany(comanda => comanda.Comensales)
            .SelectMany(comensal => comensal.Detalles)
            .ToList();

        var total = ventas.Sum(x => x.Total);
        var resumen = new ResumenVentasDiaDto(
            ventas.Count,
            detalles.Sum(x => x.Cantidad),
            total,
            ventas.Count == 0 ? 0m : decimal.Round(total / ventas.Count, 2, MidpointRounding.AwayFromZero));

        var porProducto = detalles
            .GroupBy(x => new { x.ProductoId, x.NombreProductoHistorico, x.NombreCategoriaHistorico })
            .Select(grupo =>
            {
                var importe = grupo.Sum(x => x.Subtotal);
                var cantidad = grupo.Sum(x => x.Cantidad);
                return new VentaProductoDto(
                    grupo.Key.ProductoId,
                    grupo.Key.NombreProductoHistorico,
                    grupo.Key.NombreCategoriaHistorico,
                    cantidad,
                    importe,
                    cantidad == 0 ? 0m : decimal.Round(importe / cantidad, 2, MidpointRounding.AwayFromZero));
            })
            .OrderByDescending(x => x.Importe)
            .ThenBy(x => x.Producto)
            .ToList();

        var porCategoria = detalles
            .GroupBy(x => new { x.CategoriaProductoIdHistorico, x.NombreCategoriaHistorico })
            .Select(grupo => new VentaCategoriaDto(
                grupo.Key.CategoriaProductoIdHistorico,
                grupo.Key.NombreCategoriaHistorico,
                grupo.Sum(x => x.Cantidad),
                grupo.Sum(x => x.Subtotal)))
            .OrderByDescending(x => x.Importe)
            .ThenBy(x => x.Categoria)
            .ToList();

        var porMesero = comandas
            .GroupBy(x => new { x.UsuarioMeseroId, Nombre = x.UsuarioMesero?.Nombre ?? string.Empty })
            .Select(grupo => new VentaMeseroDto(
                grupo.Key.UsuarioMeseroId,
                grupo.Key.Nombre,
                grupo.Count(),
                grupo.SelectMany(x => x.Comensales).SelectMany(x => x.Detalles).Sum(x => x.Cantidad),
                grupo.Sum(x => x.TotalCobrado ?? x.Pagos.Sum(pago => pago.Importe))))
            .OrderByDescending(x => x.Importe)
            .ThenBy(x => x.Mesero)
            .ToList();

        var porMetodoPago = comandas
            .SelectMany(x => x.Pagos)
            .GroupBy(x => x.MetodoPago.ToString())
            .Select(grupo => new VentaMetodoPagoDto(
                grupo.Key,
                grupo.Count(),
                grupo.Sum(x => x.Importe)))
            .OrderByDescending(x => x.Importe)
            .ThenBy(x => x.MetodoPago)
            .ToList();

        return new ReporteVentasDiarioDto(
            fecha,
            desfaseHorarioMinutos,
            desdeUtc,
            hastaUtcExclusiva,
            resumen,
            ventas,
            porProducto,
            porCategoria,
            porMesero,
            porMetodoPago);
    }

    private static DateTime ConvertirInicioDiaAUtc(DateOnly fecha, int desfaseHorarioMinutos)
    {
        var localSinZona = fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return DateTime.SpecifyKind(localSinZona.AddMinutes(desfaseHorarioMinutos), DateTimeKind.Utc);
    }

    private static void ValidarDesfaseHorario(int desfaseHorarioMinutos)
    {
        if (desfaseHorarioMinutos < DesfaseMinimo || desfaseHorarioMinutos > DesfaseMaximo)
        {
            throw new BusinessValidationException("El desfase horario no es válido.");
        }
    }
}
