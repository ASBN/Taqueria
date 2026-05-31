using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Taqueria.Api.Controllers;
using Taqueria.Application.DTOs.Reportes;
using Taqueria.Application.Interfaces.Services;

[ApiController]
[Authorize(Roles = "Administrador,Caja")]
[Route("api/reportes")]
public sealed class ReportesController : ControllerBase
{
    private readonly IReporteService _service;

    public ReportesController(IReporteService service)
    {
        _service = service;
    }

    [HttpGet("ventas-dia")]
    public async Task<ActionResult<ReporteVentasDiarioDto>> ObtenerVentasDia(
        [FromQuery] DateOnly? fecha = null,
        [FromQuery] int desfaseHorarioMinutos = 360,
        CancellationToken cancellationToken = default) =>
        Ok(await _service.ObtenerVentasDiaAsync(
            ResolverFecha(fecha, desfaseHorarioMinutos),
            desfaseHorarioMinutos,
            cancellationToken));

    [HttpGet("mermas-dia")]
    public async Task<ActionResult<ReporteMermasDiarioDto>> ObtenerMermasDia(
        [FromQuery] DateOnly? fecha = null,
        [FromQuery] int desfaseHorarioMinutos = 360,
        CancellationToken cancellationToken = default) =>
        Ok(await _service.ObtenerMermasDiaAsync(
            ResolverFecha(fecha, desfaseHorarioMinutos),
            desfaseHorarioMinutos,
            cancellationToken));

    [HttpGet("ventas-dia/excel")]
    public async Task<IActionResult> ExportarVentasDiaExcel(
        [FromQuery] DateOnly? fecha = null,
        [FromQuery] int desfaseHorarioMinutos = 360,
        CancellationToken cancellationToken = default)
    {
        var archivo = await _service.ExportarVentasDiaExcelAsync(
            ResolverFecha(fecha, desfaseHorarioMinutos),
            desfaseHorarioMinutos,
            cancellationToken);

        return File(archivo.Contenido, archivo.ContentType, archivo.NombreArchivo);
    }

    private static DateOnly ResolverFecha(DateOnly? fecha, int desfaseHorarioMinutos) =>
        fecha ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMinutes(-desfaseHorarioMinutos));
}
