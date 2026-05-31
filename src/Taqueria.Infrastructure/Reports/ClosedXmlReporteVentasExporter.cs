using ClosedXML.Excel;

namespace Taqueria.Infrastructure.Reports;
using Taqueria.Application.DTOs.Reportes;
using Taqueria.Application.Interfaces.Reports;

public sealed class ClosedXmlReporteVentasExporter : IReporteExcelExporter
{
    private const string ContentTypeExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public ArchivoReporteDto ExportarCorteDiario(ReporteVentasDiarioDto reporte, ReporteMermasDiarioDto mermas)
    {
        using var workbook = new XLWorkbook();

        CrearHojaResumen(workbook, reporte, mermas);
        CrearHojaVentas(workbook, reporte);
        CrearHojaPorProducto(workbook, reporte);
        CrearHojaPorCategoria(workbook, reporte);
        CrearHojaPorMesero(workbook, reporte);
        CrearHojaMermas(workbook, mermas);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        return new ArchivoReporteDto(
            $"Ventas_{reporte.Fecha:yyyy_MM_dd}.xlsx",
            ContentTypeExcel,
            stream.ToArray());
    }

    private static void CrearHojaResumen(XLWorkbook workbook, ReporteVentasDiarioDto reporte, ReporteMermasDiarioDto mermas)
    {
        var hoja = workbook.Worksheets.Add("Resumen");
        AgregarTitulo(hoja, $"Corte diario de ventas - {reporte.Fecha:dd/MM/yyyy}", 3);

        hoja.Cell(3, 1).Value = "Indicador";
        hoja.Cell(3, 2).Value = "Valor";
        AplicarEncabezado(hoja.Range(3, 1, 3, 2));

        hoja.Cell(4, 1).Value = "Ventas cobradas";
        hoja.Cell(4, 2).Value = reporte.Resumen.NumeroVentas;
        hoja.Cell(5, 1).Value = "Productos vendidos";
        hoja.Cell(5, 2).Value = reporte.Resumen.ProductosVendidos;
        hoja.Cell(6, 1).Value = "Total vendido";
        hoja.Cell(6, 2).Value = reporte.Resumen.Total;
        hoja.Cell(7, 1).Value = "Ticket promedio";
        hoja.Cell(7, 2).Value = reporte.Resumen.TicketPromedio;
        hoja.Cell(8, 1).Value = "Productos mermados";
        hoja.Cell(8, 2).Value = mermas.Resumen.ProductosMermados;
        hoja.Cell(9, 1).Value = "Importe histórico de merma";
        hoja.Cell(9, 2).Value = mermas.Resumen.ImporteHistoricoReferencia;
        hoja.Range(6, 2, 7, 2).Style.NumberFormat.Format = "$#,##0.00";
        hoja.Cell(9, 2).Style.NumberFormat.Format = "$#,##0.00";

        hoja.Cell(12, 1).Value = "Método de pago";
        hoja.Cell(12, 2).Value = "Cobros";
        hoja.Cell(12, 3).Value = "Importe";
        AplicarEncabezado(hoja.Range(12, 1, 12, 3));

        var fila = 13;
        foreach (var metodo in reporte.PorMetodoPago)
        {
            hoja.Cell(fila, 1).Value = metodo.MetodoPago;
            hoja.Cell(fila, 2).Value = metodo.NumeroVentas;
            hoja.Cell(fila, 3).Value = metodo.Importe;
            fila++;
        }

        hoja.Cell(fila, 1).Value = "TOTAL";
        hoja.Cell(fila, 3).Value = reporte.Resumen.Total;
        AplicarTotal(hoja.Range(fila, 1, fila, 3));
        hoja.Range(13, 3, fila, 3).Style.NumberFormat.Format = "$#,##0.00";
        AjustarColumnas(hoja);
    }

    private static void CrearHojaVentas(XLWorkbook workbook, ReporteVentasDiarioDto reporte)
    {
        var hoja = workbook.Worksheets.Add("Ventas del día");
        AgregarTitulo(hoja, $"Ventas cobradas - {reporte.Fecha:dd/MM/yyyy}", 8);

        var encabezados = new[] { "Fecha", "Mesa", "Comanda", "MétodoPago", "Total", "Mesero", "HoraApertura", "HoraCobro" };
        EscribirEncabezados(hoja, 3, encabezados);

        var fila = 4;
        foreach (var venta in reporte.Ventas)
        {
            hoja.Cell(fila, 1).Value = reporte.Fecha.ToDateTime(TimeOnly.MinValue);
            hoja.Cell(fila, 2).Value = venta.Mesa;
            hoja.Cell(fila, 3).Value = venta.Comanda;
            hoja.Cell(fila, 4).Value = venta.MetodoPago;
            hoja.Cell(fila, 5).Value = venta.Total;
            hoja.Cell(fila, 6).Value = venta.Mesero;
            hoja.Cell(fila, 7).Value = ConvertirHoraLocal(venta.HoraAperturaUtc, reporte.DesfaseHorarioMinutos);
            hoja.Cell(fila, 8).Value = ConvertirHoraLocal(venta.HoraCobroUtc, reporte.DesfaseHorarioMinutos);
            fila++;
        }

        hoja.Cell(fila, 4).Value = "TOTAL";
        hoja.Cell(fila, 5).Value = reporte.Resumen.Total;
        AplicarTotal(hoja.Range(fila, 1, fila, 8));
        hoja.Column(1).Style.NumberFormat.Format = "dd/mm/yyyy";
        hoja.Column(5).Style.NumberFormat.Format = "$#,##0.00";
        hoja.Column(7).Style.NumberFormat.Format = "hh:mm";
        hoja.Column(8).Style.NumberFormat.Format = "hh:mm";
        AjustarColumnas(hoja);
    }

    private static void CrearHojaPorProducto(XLWorkbook workbook, ReporteVentasDiarioDto reporte)
    {
        var hoja = workbook.Worksheets.Add("Por producto");
        AgregarTitulo(hoja, "Ventas por producto", 5);
        EscribirEncabezados(hoja, 3, new[] { "Producto", "Categoría histórica", "Cantidad", "Precio promedio", "Importe" });

        var fila = 4;
        foreach (var item in reporte.PorProducto)
        {
            hoja.Cell(fila, 1).Value = item.Producto;
            hoja.Cell(fila, 2).Value = item.Categoria;
            hoja.Cell(fila, 3).Value = item.Cantidad;
            hoja.Cell(fila, 4).Value = item.PrecioPromedio;
            hoja.Cell(fila, 5).Value = item.Importe;
            fila++;
        }

        hoja.Cell(fila, 1).Value = "TOTAL";
        hoja.Cell(fila, 3).Value = reporte.Resumen.ProductosVendidos;
        hoja.Cell(fila, 5).Value = reporte.Resumen.Total;
        AplicarTotal(hoja.Range(fila, 1, fila, 5));
        hoja.Column(4).Style.NumberFormat.Format = "$#,##0.00";
        hoja.Column(5).Style.NumberFormat.Format = "$#,##0.00";
        AjustarColumnas(hoja);
    }

    private static void CrearHojaPorCategoria(XLWorkbook workbook, ReporteVentasDiarioDto reporte)
    {
        var hoja = workbook.Worksheets.Add("Por categoría");
        AgregarTitulo(hoja, "Ventas por categoría histórica", 3);
        EscribirEncabezados(hoja, 3, new[] { "Categoría", "Cantidad", "Importe" });

        var fila = 4;
        foreach (var item in reporte.PorCategoria)
        {
            hoja.Cell(fila, 1).Value = item.Categoria;
            hoja.Cell(fila, 2).Value = item.Cantidad;
            hoja.Cell(fila, 3).Value = item.Importe;
            fila++;
        }

        hoja.Cell(fila, 1).Value = "TOTAL";
        hoja.Cell(fila, 2).Value = reporte.Resumen.ProductosVendidos;
        hoja.Cell(fila, 3).Value = reporte.Resumen.Total;
        AplicarTotal(hoja.Range(fila, 1, fila, 3));
        hoja.Column(3).Style.NumberFormat.Format = "$#,##0.00";
        AjustarColumnas(hoja);
    }

    private static void CrearHojaPorMesero(XLWorkbook workbook, ReporteVentasDiarioDto reporte)
    {
        var hoja = workbook.Worksheets.Add("Por mesero");
        AgregarTitulo(hoja, "Ventas por mesero", 4);
        EscribirEncabezados(hoja, 3, new[] { "Mesero", "Ventas", "Productos", "Importe" });

        var fila = 4;
        foreach (var item in reporte.PorMesero)
        {
            hoja.Cell(fila, 1).Value = item.Mesero;
            hoja.Cell(fila, 2).Value = item.NumeroVentas;
            hoja.Cell(fila, 3).Value = item.ProductosVendidos;
            hoja.Cell(fila, 4).Value = item.Importe;
            fila++;
        }

        hoja.Cell(fila, 1).Value = "TOTAL";
        hoja.Cell(fila, 2).Value = reporte.Resumen.NumeroVentas;
        hoja.Cell(fila, 3).Value = reporte.Resumen.ProductosVendidos;
        hoja.Cell(fila, 4).Value = reporte.Resumen.Total;
        AplicarTotal(hoja.Range(fila, 1, fila, 4));
        hoja.Column(4).Style.NumberFormat.Format = "$#,##0.00";
        AjustarColumnas(hoja);
    }

    private static void CrearHojaMermas(XLWorkbook workbook, ReporteMermasDiarioDto reporte)
    {
        var hoja = workbook.Worksheets.Add("Mermas");
        AgregarTitulo(hoja, $"Mermas por cancelación - {reporte.Fecha:dd/MM/yyyy}", 9);
        EscribirEncabezados(hoja, 3, new[] { "Hora", "Mesa", "Comanda", "Producto", "Categoría", "Cantidad", "Importe histórico", "Motivo", "Registrada por" });

        var fila = 4;
        foreach (var item in reporte.Mermas)
        {
            hoja.Cell(fila, 1).Value = ConvertirHoraLocal(item.FechaHoraRegistroUtc, reporte.DesfaseHorarioMinutos);
            hoja.Cell(fila, 2).Value = item.Mesa;
            hoja.Cell(fila, 3).Value = item.Comanda;
            hoja.Cell(fila, 4).Value = item.Producto;
            hoja.Cell(fila, 5).Value = item.Categoria;
            hoja.Cell(fila, 6).Value = item.Cantidad;
            hoja.Cell(fila, 7).Value = item.ImporteHistorico;
            hoja.Cell(fila, 8).Value = item.Motivo;
            hoja.Cell(fila, 9).Value = item.RegistradaPor;
            fila++;
        }

        hoja.Cell(fila, 5).Value = "TOTAL";
        hoja.Cell(fila, 6).Value = reporte.Resumen.ProductosMermados;
        hoja.Cell(fila, 7).Value = reporte.Resumen.ImporteHistoricoReferencia;
        AplicarTotal(hoja.Range(fila, 1, fila, 9));
        hoja.Column(1).Style.NumberFormat.Format = "hh:mm";
        hoja.Column(7).Style.NumberFormat.Format = "$#,##0.00";
        AjustarColumnas(hoja);
    }

    private static void AgregarTitulo(IXLWorksheet hoja, string titulo, int numeroColumnas)
    {
        hoja.Cell(1, 1).Value = titulo;
        hoja.Range(1, 1, 1, numeroColumnas).Merge();
        hoja.Cell(1, 1).Style.Font.Bold = true;
        hoja.Cell(1, 1).Style.Font.FontSize = 16;
        hoja.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#7D2514");
    }

    private static void EscribirEncabezados(IXLWorksheet hoja, int fila, IReadOnlyList<string> encabezados)
    {
        for (var columna = 0; columna < encabezados.Count; columna++)
        {
            hoja.Cell(fila, columna + 1).Value = encabezados[columna];
        }

        AplicarEncabezado(hoja.Range(fila, 1, fila, encabezados.Count));
    }

    private static void AplicarEncabezado(IXLRange rango)
    {
        rango.Style.Font.Bold = true;
        rango.Style.Font.FontColor = XLColor.White;
        rango.Style.Fill.BackgroundColor = XLColor.FromHtml("#B23A22");
        rango.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
    }

    private static void AplicarTotal(IXLRange rango)
    {
        rango.Style.Font.Bold = true;
        rango.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF2ED");
        rango.Style.Border.TopBorder = XLBorderStyleValues.Thin;
    }

    private static void AjustarColumnas(IXLWorksheet hoja)
    {
        hoja.ColumnsUsed().AdjustToContents();
        hoja.SheetView.FreezeRows(3);
    }

    private static DateTime ConvertirHoraLocal(DateTime fechaUtc, int desfaseHorarioMinutos) =>
        fechaUtc.AddMinutes(-desfaseHorarioMinutos);
}
