import { useCallback, useEffect, useMemo, useState } from 'react';
import { Button, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components';
import type { CobroRegistrado, CuentaPendienteCobro } from '../models/api';
import { apiClient } from '../services/apiClient';
import { currency } from '../utils/format';

type MetodoPago = 'Efectivo' | 'Tarjeta' | 'Transferencia';
type ImportesMixtos = Record<MetodoPago, string>;

const metodos: MetodoPago[] = ['Efectivo', 'Tarjeta', 'Transferencia'];
const importesVacios: ImportesMixtos = { Efectivo: '', Tarjeta: '', Transferencia: '' };

export function CajaPage() {
  const [cuentas, setCuentas] = useState<CuentaPendienteCobro[]>([]);
  const [cargando, setCargando] = useState(true);
  const [procesandoId, setProcesandoId] = useState<number | null>(null);
  const [cuentaMixtaId, setCuentaMixtaId] = useState<number | null>(null);
  const [importes, setImportes] = useState<ImportesMixtos>(importesVacios);
  const [mensaje, setMensaje] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const cargar = useCallback(async (mostrarSpinner = false) => {
    if (mostrarSpinner) setCargando(true);
    try {
      const response = await apiClient.get<CuentaPendienteCobro[]>('/api/cobros/pendientes');
      setCuentas(response);
      setError(null);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible cargar las cuentas pendientes.');
    } finally {
      if (mostrarSpinner) setCargando(false);
    }
  }, []);

  useEffect(() => {
    void cargar(true);
    const interval = window.setInterval(() => void cargar(), 15000);
    return () => window.clearInterval(interval);
  }, [cargar]);

  async function cobrarMetodoUnico(cuenta: CuentaPendienteCobro, metodoPago: MetodoPago) {
    if (!window.confirm(`¿Registrar cobro de ${currency(cuenta.total)} en ${metodoPago} para la mesa ${cuenta.mesaNumero}?`)) {
      return;
    }

    await ejecutarCobro(cuenta, { metodoPago });
  }

  async function cobrarMixto(cuenta: CuentaPendienteCobro) {
    const pagos = metodos
      .map((metodoPago) => ({ metodoPago, importe: redondear(Number(importes[metodoPago] || 0)) }))
      .filter((pago) => pago.importe > 0);

    const totalDistribuido = redondear(pagos.reduce((suma, pago) => suma + pago.importe, 0));
    if (totalDistribuido !== redondear(cuenta.total)) {
      setError('La suma del pago mixto debe coincidir exactamente con el total de la cuenta.');
      return;
    }

    const detalle = pagos.map((pago) => `${pago.metodoPago}: ${currency(pago.importe)}`).join(' · ');
    if (!window.confirm(`¿Registrar pago mixto para la mesa ${cuenta.mesaNumero}?\n${detalle}`)) {
      return;
    }

    await ejecutarCobro(cuenta, { pagos });
  }

  async function ejecutarCobro(cuenta: CuentaPendienteCobro, body: unknown) {
    setProcesandoId(cuenta.comandaId);
    setError(null);
    setMensaje(null);
    try {
      const cobro = await apiClient.post<CobroRegistrado>(`/api/cobros/comandas/${cuenta.comandaId}`, body);
      setCuentas((actuales) => actuales.filter((item) => item.comandaId !== cuenta.comandaId));
      setCuentaMixtaId(null);
      setImportes(importesVacios);
      const desglose = cobro.pagos.map((pago) => `${pago.metodoPago} ${currency(pago.importe)}`).join(' + ');
      setMensaje(`Mesa ${cobro.mesaNumero} cobrada: ${desglose}. Total ${currency(cobro.totalCobrado)}. La mesa ya está libre.`);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible registrar el cobro.');
    } finally {
      setProcesandoId(null);
    }
  }

  function iniciarMixto(cuentaId: number) {
    setCuentaMixtaId(cuentaId);
    setImportes(importesVacios);
    setError(null);
  }

  if (cargando) {
    return <div className="text-center py-5"><Spinner label="Cargando caja..." /></div>;
  }

  return (
    <div className="mx-auto page-width">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <div>
          <h1 className="h4 mb-1">Caja</h1>
          <p className="small text-secondary mb-0">Cobro exacto, sencillo o mixto, de cuentas enviadas por mesero</p>
        </div>
        <Button appearance="secondary" onClick={() => void cargar()}>Actualizar</Button>
      </div>

      {mensaje && <MessageBar intent="success" className="mb-3"><MessageBarBody>{mensaje}</MessageBarBody></MessageBar>}
      {error && <MessageBar intent="error" className="mb-3"><MessageBarBody>{error}</MessageBarBody></MessageBar>}

      {cuentas.length === 0 ? (
        <div className="card border-0 shadow-sm">
          <div className="card-body text-center py-5 text-secondary">
            <i className="bi bi-check2-circle fs-1 d-block mb-2" />
            No hay cuentas pendientes de cobro.
          </div>
        </div>
      ) : (
        <div className="d-grid gap-3">
          {cuentas.map((cuenta) => (
            <CuentaCajaCard
              key={cuenta.comandaId}
              cuenta={cuenta}
              mixtaActiva={cuentaMixtaId === cuenta.comandaId}
              importes={importes}
              procesando={procesandoId === cuenta.comandaId}
              onMetodoUnico={cobrarMetodoUnico}
              onIniciarMixto={iniciarMixto}
              onCerrarMixto={() => setCuentaMixtaId(null)}
              onImporte={(metodo, importe) => setImportes((actuales) => ({ ...actuales, [metodo]: importe }))}
              onCobrarMixto={cobrarMixto}
            />
          ))}
        </div>
      )}
    </div>
  );
}

interface CuentaCajaCardProps {
  cuenta: CuentaPendienteCobro;
  mixtaActiva: boolean;
  importes: ImportesMixtos;
  procesando: boolean;
  onMetodoUnico: (cuenta: CuentaPendienteCobro, metodo: MetodoPago) => Promise<void>;
  onIniciarMixto: (id: number) => void;
  onCerrarMixto: () => void;
  onImporte: (metodo: MetodoPago, importe: string) => void;
  onCobrarMixto: (cuenta: CuentaPendienteCobro) => Promise<void>;
}

function CuentaCajaCard({ cuenta, mixtaActiva, importes, procesando, onMetodoUnico, onIniciarMixto, onCerrarMixto, onImporte, onCobrarMixto }: CuentaCajaCardProps) {
  const totalDistribuido = useMemo(
    () => redondear(metodos.reduce((suma, metodo) => suma + Number(importes[metodo] || 0), 0)),
    [importes]
  );
  const diferencia = redondear(cuenta.total - totalDistribuido);
  const coincide = diferencia === 0 && totalDistribuido > 0;

  return (
    <article className="card cashier-card border-0 shadow-sm">
      <div className="card-body">
        <div className="d-flex justify-content-between align-items-start mb-3">
          <div>
            <div className="small text-secondary text-uppercase">Mesa</div>
            <div className="cashier-table-number">{cuenta.mesaNumero}</div>
            <div className="small text-secondary">Mesero: {cuenta.mesero}</div>
          </div>
          <div className="text-end">
            <span className="badge text-bg-warning mb-2">Pendiente de cobro</span>
            <div className="small text-secondary">Total a cobrar</div>
            <div className="fs-2 fw-bold text-brand">{currency(cuenta.total)}</div>
          </div>
        </div>
        <div className="small text-secondary mb-3">
          {cuenta.totalProductos} producto(s) en {cuenta.totalPartidas} partida(s) · Entregada {formatearHora(cuenta.fechaHoraEntregaUtc)}
        </div>

        {!mixtaActiva ? (
          <>
            <div className="cashier-payment-buttons">
              {metodos.map((metodo) => (
                <Button
                  key={metodo}
                  appearance={metodo === 'Efectivo' ? 'primary' : 'secondary'}
                  size="large"
                  disabled={procesando}
                  onClick={() => void onMetodoUnico(cuenta, metodo)}
                >
                  {metodo}
                </Button>
              ))}
            </div>
            <Button appearance="subtle" className="w-100 mt-2" disabled={procesando} onClick={() => onIniciarMixto(cuenta.comandaId)}>
              Pago mixto
            </Button>
          </>
        ) : (
          <div className="mixed-payment-panel mt-3">
            <h2 className="h6">Distribución de pago mixto</h2>
            {metodos.map((metodo) => (
              <label className="d-flex align-items-center justify-content-between gap-3 mb-2" key={metodo}>
                <span>{metodo}</span>
                <input
                  className="form-control text-end mixed-payment-input"
                  type="number"
                  inputMode="decimal"
                  min="0"
                  step="0.01"
                  value={importes[metodo]}
                  onChange={(event) => onImporte(metodo, event.target.value)}
                  placeholder="$0.00"
                />
              </label>
            ))}
            <div className={`alert py-2 mt-3 mb-3 ${coincide ? 'alert-success' : 'alert-warning'}`}>
              {coincide
                ? `Distribución completa: ${currency(totalDistribuido)}`
                : diferencia > 0
                  ? `Falta asignar ${currency(diferencia)}`
                  : `Excede por ${currency(Math.abs(diferencia))}`}
            </div>
            <div className="d-flex gap-2">
              <Button appearance="primary" className="flex-grow-1" disabled={procesando || !coincide} onClick={() => void onCobrarMixto(cuenta)}>
                Registrar pago mixto
              </Button>
              <Button appearance="secondary" disabled={procesando} onClick={onCerrarMixto}>Cancelar</Button>
            </div>
          </div>
        )}
      </div>
    </article>
  );
}

function redondear(value: number) {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}

function formatearHora(value: string | null) {
  if (!value) return '';
  return new Date(value).toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' });
}
