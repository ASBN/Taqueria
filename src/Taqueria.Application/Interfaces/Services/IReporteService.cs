namespace Taqueria.Application.Interfaces.Services;
using Taqueria.Application.DTOs.Reportes;

public interface IReporteService
{
    Task<ReporteVentasDiarioDto> ObtenerVentasDiaAsync(
        DateOnly fecha,
        int desfaseHorarioMinutos,
        CancellationToken cancellationToken = default);

    Task<ReporteMermasDiarioDto> ObtenerMermasDiaAsync(
        DateOnly fecha,
        int desfaseHorarioMinutos,
        CancellationToken cancellationToken = default);

    Task<ArchivoReporteDto> ExportarVentasDiaExcelAsync(
        DateOnly fecha,
        int desfaseHorarioMinutos,
        CancellationToken cancellationToken = default);
}
