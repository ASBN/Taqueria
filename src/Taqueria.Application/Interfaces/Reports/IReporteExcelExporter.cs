namespace Taqueria.Application.Interfaces.Reports;
using Taqueria.Application.DTOs.Reportes;

public interface IReporteExcelExporter
{
    ArchivoReporteDto ExportarCorteDiario(ReporteVentasDiarioDto ventas, ReporteMermasDiarioDto mermas);
}
