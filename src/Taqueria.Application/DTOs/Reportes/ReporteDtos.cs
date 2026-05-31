namespace Taqueria.Application.DTOs.Reportes;

public sealed record ResumenVentasDiaDto(
    int NumeroVentas,
    int ProductosVendidos,
    decimal Total,
    decimal TicketPromedio);

public sealed record VentaDiaDto(
    DateTime FechaHoraCobroUtc,
    int Mesa,
    string Comanda,
    string MetodoPago,
    decimal Total,
    string Mesero,
    DateTime HoraAperturaUtc,
    DateTime HoraCobroUtc);

public sealed record VentaProductoDto(
    int ProductoId,
    string Producto,
    string Categoria,
    int Cantidad,
    decimal Importe,
    decimal PrecioPromedio);

public sealed record VentaCategoriaDto(
    int CategoriaProductoId,
    string Categoria,
    int Cantidad,
    decimal Importe);

public sealed record VentaMeseroDto(
    int UsuarioMeseroId,
    string Mesero,
    int NumeroVentas,
    int ProductosVendidos,
    decimal Importe);

public sealed record VentaMetodoPagoDto(
    string MetodoPago,
    int NumeroVentas,
    decimal Importe);

public sealed record ReporteVentasDiarioDto(
    DateOnly Fecha,
    int DesfaseHorarioMinutos,
    DateTime DesdeUtc,
    DateTime HastaUtcExclusiva,
    ResumenVentasDiaDto Resumen,
    IReadOnlyList<VentaDiaDto> Ventas,
    IReadOnlyList<VentaProductoDto> PorProducto,
    IReadOnlyList<VentaCategoriaDto> PorCategoria,
    IReadOnlyList<VentaMeseroDto> PorMesero,
    IReadOnlyList<VentaMetodoPagoDto> PorMetodoPago);

public sealed record ArchivoReporteDto(
    string NombreArchivo,
    string ContentType,
    byte[] Contenido);

public sealed record ResumenMermasDiaDto(
    int NumeroCancelacionesConMerma,
    int ProductosMermados,
    decimal ImporteHistoricoReferencia);

public sealed record MermaDiaDto(
    DateTime FechaHoraRegistroUtc,
    int Mesa,
    string Comanda,
    string Producto,
    string Categoria,
    int Cantidad,
    decimal ImporteHistorico,
    string Motivo,
    string RegistradaPor);

public sealed record ReporteMermasDiarioDto(
    DateOnly Fecha,
    int DesfaseHorarioMinutos,
    DateTime DesdeUtc,
    DateTime HastaUtcExclusiva,
    ResumenMermasDiaDto Resumen,
    IReadOnlyList<MermaDiaDto> Mermas);
