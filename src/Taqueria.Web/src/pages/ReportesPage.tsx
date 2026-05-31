import { useEffect, useMemo, useState } from 'react';
import { Button, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components';
import type { ReporteMermasDiario, ReporteVentasDiario } from '../models/api';
import { apiClient } from '../services/apiClient';
import { currency, timeLocal, todayLocalInputValue } from '../utils/format';

type VistaReporte = 'ventas' | 'producto' | 'categoria' | 'mesero' | 'mermas';

export function ReportesPage() {
  const [fecha, setFecha] = useState(todayLocalInputValue());
  const [reporte, setReporte] = useState<ReporteVentasDiario | null>(null);
  const [mermas, setMermas] = useState<ReporteMermasDiario | null>(null);
  const [vista, setVista] = useState<VistaReporte>('ventas');
  const [cargando, setCargando] = useState(true);
  const [descargando, setDescargando] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const query = useMemo(() => {
    const desfase = new Date(`${fecha}T12:00:00`).getTimezoneOffset();
    return `fecha=${encodeURIComponent(fecha)}&desfaseHorarioMinutos=${desfase}`;
  }, [fecha]);

  useEffect(() => {
    void cargar();
  // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [query]);

  async function cargar() {
    setCargando(true);
    setError(null);
    try {
      const [ventasResponse, mermasResponse] = await Promise.all([
        apiClient.get<ReporteVentasDiario>(`/api/reportes/ventas-dia?${query}`),
        apiClient.get<ReporteMermasDiario>(`/api/reportes/mermas-dia?${query}`)
      ]);
      setReporte(ventasResponse);
      setMermas(mermasResponse);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible consultar el reporte.');
    } finally {
      setCargando(false);
    }
  }

  async function descargarExcel() {
    setDescargando(true);
    setError(null);
    try {
      const archivo = await apiClient.download(`/api/reportes/ventas-dia/excel?${query}`);
      const url = URL.createObjectURL(archivo.blob);
      const enlace = document.createElement('a');
      enlace.href = url;
      enlace.download = archivo.filename;
      enlace.click();
      URL.revokeObjectURL(url);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible descargar el corte diario.');
    } finally {
      setDescargando(false);
    }
  }

  return (
    <div className="mx-auto report-width">
      <div className="d-flex flex-wrap justify-content-between align-items-end gap-3 mb-3">
        <div>
          <h1 className="h4 mb-1">Reportes</h1>
          <p className="small text-secondary mb-0">Ventas cobradas, métodos de pago y mermas por cancelación</p>
        </div>
        <div className="d-flex gap-2 align-items-end">
          <label className="small text-secondary">
            Fecha
            <input className="form-control mt-1" type="date" required value={fecha} onChange={(event) => event.target.value && setFecha(event.target.value)} />
          </label>
          <Button appearance="primary" disabled={descargando} onClick={() => void descargarExcel()}>
            <i className="bi bi-file-earmark-spreadsheet me-2" />
            {descargando ? 'Generando...' : 'Excel'}
          </Button>
        </div>
      </div>

      {error && <MessageBar intent="error" className="mb-3"><MessageBarBody>{error}</MessageBarBody></MessageBar>}
      {cargando && <div className="text-center py-5"><Spinner label="Calculando corte..." /></div>}
      {!cargando && reporte && mermas && <ContenidoReporte reporte={reporte} mermas={mermas} vista={vista} onVista={setVista} />}
    </div>
  );
}

function ContenidoReporte({ reporte, mermas, vista, onVista }: {
  reporte: ReporteVentasDiario;
  mermas: ReporteMermasDiario;
  vista: VistaReporte;
  onVista(vista: VistaReporte): void;
}) {
  return (
    <>
      <div className="row g-2 report-summary mb-3">
        <ResumenCard etiqueta="Ventas" valor={reporte.resumen.numeroVentas.toString()} />
        <ResumenCard etiqueta="Total" valor={currency(reporte.resumen.total)} />
        <ResumenCard etiqueta="Productos" valor={reporte.resumen.productosVendidos.toString()} />
        <ResumenCard etiqueta="Ticket prom." valor={currency(reporte.resumen.ticketPromedio)} />
        <ResumenCard etiqueta="Merma uds." valor={mermas.resumen.productosMermados.toString()} />
        <ResumenCard etiqueta="Merma ref." valor={currency(mermas.resumen.importeHistoricoReferencia)} />
      </div>

      <section className="card border-0 shadow-sm mb-3">
        <div className="card-body">
          <h2 className="h6 mb-3">Por método de pago</h2>
          {reporte.porMetodoPago.length === 0 ? (
            <div className="text-secondary small">Sin cobros registrados este día.</div>
          ) : reporte.porMetodoPago.map((metodo) => (
            <div className="payment-summary-row" key={metodo.metodoPago}>
              <span>{metodo.metodoPago} <small className="text-secondary">({metodo.numeroVentas} cobro(s))</small></span>
              <strong>{currency(metodo.importe)}</strong>
            </div>
          ))}
        </div>
      </section>

      <div className="report-tabs mb-3" role="tablist" aria-label="Detalle del corte">
        {([
          ['ventas', 'Ventas'],
          ['producto', 'Producto'],
          ['categoria', 'Categoría'],
          ['mesero', 'Mesero'],
          ['mermas', 'Mermas']
        ] as [VistaReporte, string][]).map(([id, texto]) => (
          <button type="button" key={id} className={vista === id ? 'selected' : ''} onClick={() => onVista(id)}>
            {texto}
          </button>
        ))}
      </div>

      <section className="card border-0 shadow-sm report-table-card">
        <div className="table-responsive">
          {vista === 'ventas' && <TablaVentas reporte={reporte} />}
          {vista === 'producto' && <TablaProducto reporte={reporte} />}
          {vista === 'categoria' && <TablaCategoria reporte={reporte} />}
          {vista === 'mesero' && <TablaMesero reporte={reporte} />}
          {vista === 'mermas' && <TablaMermas reporte={mermas} />}
        </div>
      </section>
    </>
  );
}

function ResumenCard({ etiqueta, valor }: { etiqueta: string; valor: string }) {
  return (
    <div className="col-6 col-md-4 col-lg-2">
      <article className="card border-0 shadow-sm h-100">
        <div className="card-body p-3">
          <div className="small text-secondary">{etiqueta}</div>
          <div className="fw-bold report-value">{valor}</div>
        </div>
      </article>
    </div>
  );
}

function TablaVentas({ reporte }: { reporte: ReporteVentasDiario }) {
  return (
    <table className="table table-sm align-middle mb-0">
      <thead><tr><th>Hora</th><th>Mesa</th><th>Método</th><th className="text-end">Total</th></tr></thead>
      <tbody>
        {reporte.ventas.map((venta) => (
          <tr key={venta.comanda}>
            <td>{timeLocal(venta.horaCobroUtc)}</td>
            <td>{venta.mesa}<div className="small text-secondary">{venta.mesero}</div></td>
            <td>{venta.metodoPago}</td>
            <td className="text-end fw-semibold">{currency(venta.total)}</td>
          </tr>
        ))}
        {reporte.ventas.length === 0 && <FilaSinVentas />}
      </tbody>
    </table>
  );
}

function TablaProducto({ reporte }: { reporte: ReporteVentasDiario }) {
  return (
    <table className="table table-sm align-middle mb-0">
      <thead><tr><th>Producto</th><th className="text-end">Cant.</th><th className="text-end">Importe</th></tr></thead>
      <tbody>
        {reporte.porProducto.map((item) => (
          <tr key={`${item.productoId}-${item.categoria}`}>
            <td>{item.producto}<div className="small text-secondary">{item.categoria}</div></td>
            <td className="text-end">{item.cantidad}</td>
            <td className="text-end fw-semibold">{currency(item.importe)}</td>
          </tr>
        ))}
        {reporte.porProducto.length === 0 && <FilaSinVentas />}
      </tbody>
    </table>
  );
}

function TablaCategoria({ reporte }: { reporte: ReporteVentasDiario }) {
  return (
    <table className="table table-sm align-middle mb-0">
      <thead><tr><th>Categoría histórica</th><th className="text-end">Cant.</th><th className="text-end">Importe</th></tr></thead>
      <tbody>
        {reporte.porCategoria.map((item) => (
          <tr key={item.categoriaProductoId}>
            <td>{item.categoria}</td>
            <td className="text-end">{item.cantidad}</td>
            <td className="text-end fw-semibold">{currency(item.importe)}</td>
          </tr>
        ))}
        {reporte.porCategoria.length === 0 && <FilaSinVentas />}
      </tbody>
    </table>
  );
}

function TablaMesero({ reporte }: { reporte: ReporteVentasDiario }) {
  return (
    <table className="table table-sm align-middle mb-0">
      <thead><tr><th>Mesero</th><th className="text-end">Ventas</th><th className="text-end">Importe</th></tr></thead>
      <tbody>
        {reporte.porMesero.map((item) => (
          <tr key={item.usuarioMeseroId}>
            <td>{item.mesero}<div className="small text-secondary">{item.productosVendidos} producto(s)</div></td>
            <td className="text-end">{item.numeroVentas}</td>
            <td className="text-end fw-semibold">{currency(item.importe)}</td>
          </tr>
        ))}
        {reporte.porMesero.length === 0 && <FilaSinVentas />}
      </tbody>
    </table>
  );
}

function TablaMermas({ reporte }: { reporte: ReporteMermasDiario }) {
  return (
    <table className="table table-sm align-middle mb-0">
      <thead><tr><th>Hora / Mesa</th><th>Producto</th><th className="text-end">Cant.</th><th className="text-end">Referencia</th></tr></thead>
      <tbody>
        {reporte.mermas.map((item) => (
          <tr key={`${item.comanda}-${item.producto}-${item.fechaHoraRegistroUtc}`}>
            <td>{timeLocal(item.fechaHoraRegistroUtc)} · Mesa {item.mesa}<div className="small text-secondary">{item.registradaPor}</div></td>
            <td>{item.producto}<div className="small text-secondary">{item.motivo}</div></td>
            <td className="text-end">{item.cantidad}</td>
            <td className="text-end fw-semibold">{currency(item.importeHistorico)}</td>
          </tr>
        ))}
        {reporte.mermas.length === 0 && <tr><td className="text-secondary text-center py-4" colSpan={4}>No hay mermas registradas para la fecha seleccionada.</td></tr>}
      </tbody>
    </table>
  );
}

function FilaSinVentas() {
  return <tr><td className="text-secondary text-center py-4" colSpan={4}>No hay ventas cobradas para la fecha seleccionada.</td></tr>;
}
