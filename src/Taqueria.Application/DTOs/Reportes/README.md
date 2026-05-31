# DTOs de Reportes

Los DTOs de este módulo representan el corte diario:

- `ReporteVentasDiarioDto`: ventas cobradas para UI y Excel.
- `ResumenVentasDiaDto`: ventas, productos, total y ticket promedio.
- `VentaDiaDto`: detalle por comanda; en pagos múltiples muestra método `Mixto`.
- `VentaProductoDto`, `VentaCategoriaDto`, `VentaMeseroDto`, `VentaMetodoPagoDto`: agrupados; método de pago agrupa cada participación real del cobro.
- `ReporteMermasDiarioDto` y `MermaDiaDto`: mermas creadas por cancelaciones con producto preparado no entregado.
- `ArchivoReporteDto`: bytes y metadatos de la exportación Excel.

Categoría, precio e importe de referencia de la merma provienen de los snapshots históricos de `ComandaDetalle`.
