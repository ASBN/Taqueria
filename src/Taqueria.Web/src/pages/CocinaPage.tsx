import { useEffect, useMemo, useState } from 'react';
import { Button, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components';
import type { ComandaCocina, PartidaCocina } from '../models/api';
import { apiClient } from '../services/apiClient';

interface CantidadesLiberacion {
  [detalleId: number]: number;
}

type VistaCocina = 'pendientes' | 'listas';

export function CocinaPage() {
  const [comandas, setComandas] = useState<ComandaCocina[]>([]);
  const [vista, setVista] = useState<VistaCocina>('pendientes');
  const [cantidades, setCantidades] = useState<CantidadesLiberacion>({});
  const [cargando, setCargando] = useState(true);
  const [procesandoId, setProcesandoId] = useState<number | null>(null);
  const [mensaje, setMensaje] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function cargar(mostrarCarga = true) {
    if (mostrarCarga) setCargando(true);
    setError(null);
    try {
      const response = await apiClient.get<ComandaCocina[]>('/api/cocina/comandas?incluirListas=true');
      setComandas(response);
      setCantidades((actuales) => inicializarCantidades(response, actuales));
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible cargar la cocina.');
    } finally {
      if (mostrarCarga) setCargando(false);
    }
  }

  useEffect(() => {
    void cargar();
    const intervalId = window.setInterval(() => void cargar(false), 15000);
    return () => window.clearInterval(intervalId);
  }, []);

  const pendientes = useMemo(
    () => comandas.filter((comanda) => comanda.estado !== 'Lista'),
    [comandas]
  );

  const listas = useMemo(
    () => comandas.filter((comanda) => comanda.estado === 'Lista'),
    [comandas]
  );

  const comandasVisibles = vista === 'pendientes' ? pendientes : listas;

  function cambiarCantidad(detalle: PartidaCocina, cantidad: number) {
    const valor = Math.max(1, Math.min(detalle.cantidadPendiente, Number.isFinite(cantidad) ? cantidad : 1));
    setCantidades((actuales) => ({ ...actuales, [detalle.id]: valor }));
  }

  async function liberar(comanda: ComandaCocina, detalle: PartidaCocina) {
    const cantidad = cantidades[detalle.id] ?? detalle.cantidadPendiente;
    setProcesandoId(detalle.id);
    setError(null);
    setMensaje(null);
    try {
      const actualizada = await apiClient.post<ComandaCocina>(
        `/api/cocina/comandas/${comanda.id}/partidas/${detalle.id}/entregas-parciales`,
        { cantidad }
      );
      setComandas((actuales) => actuales.map((item) => item.id === actualizada.id ? actualizada : item));
      setCantidades((actuales) => inicializarCantidades([actualizada], actuales));
      setMensaje(`${cantidad} × ${detalle.producto} liberados para mesa ${comanda.mesaNumero}.`);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible liberar la partida.');
    } finally {
      setProcesandoId(null);
    }
  }

  if (cargando) {
    return <div className="text-center py-5"><Spinner label="Cargando cocina..." /></div>;
  }

  return (
    <div className="mx-auto kitchen-width">
      <div className="d-flex align-items-start justify-content-between gap-2 mb-3">
        <div>
          <h1 className="h4 mb-1">Cocina</h1>
          <p className="text-secondary small mb-0">Liberación por partidas · actualización automática cada 15 s</p>
        </div>
        <Button appearance="secondary" icon={<i className="bi bi-arrow-clockwise" />} onClick={() => void cargar()}>
          Actualizar
        </Button>
      </div>

      {mensaje && (
        <MessageBar intent="success" className="mb-3"><MessageBarBody>{mensaje}</MessageBarBody></MessageBar>
      )}
      {error && (
        <MessageBar intent="error" className="mb-3"><MessageBarBody>{error}</MessageBarBody></MessageBar>
      )}

      <div className="kitchen-summary row g-2 mb-3">
        <div className="col-6">
          <button type="button" className={`card border-0 shadow-sm w-100 ${vista === 'pendientes' ? 'selected' : ''}`} onClick={() => setVista('pendientes')}>
            <div className="card-body text-start">
              <span className="small text-secondary">Pendientes</span>
              <strong>{pendientes.length}</strong>
            </div>
          </button>
        </div>
        <div className="col-6">
          <button type="button" className={`card border-0 shadow-sm w-100 ${vista === 'listas' ? 'selected' : ''}`} onClick={() => setVista('listas')}>
            <div className="card-body text-start">
              <span className="small text-secondary">Listas</span>
              <strong>{listas.length}</strong>
            </div>
          </button>
        </div>
      </div>

      {comandasVisibles.length === 0 ? (
        <div className="card border-0 shadow-sm">
          <div className="card-body text-center py-5 text-secondary">
            <i className="bi bi-check2-circle fs-1 d-block mb-2" />
            {vista === 'pendientes' ? 'No hay órdenes pendientes en cocina.' : 'Aún no hay órdenes completamente listas.'}
          </div>
        </div>
      ) : (
        <div className="d-grid gap-3">
          {comandasVisibles.map((comanda) => (
            <ComandaCocinaCard
              key={comanda.id}
              comanda={comanda}
              cantidades={cantidades}
              procesandoId={procesandoId}
              onCantidad={cambiarCantidad}
              onLiberar={liberar}
            />
          ))}
        </div>
      )}
    </div>
  );
}

interface ComandaCocinaCardProps {
  comanda: ComandaCocina;
  cantidades: CantidadesLiberacion;
  procesandoId: number | null;
  onCantidad: (detalle: PartidaCocina, cantidad: number) => void;
  onLiberar: (comanda: ComandaCocina, detalle: PartidaCocina) => Promise<void>;
}

function ComandaCocinaCard({ comanda, cantidades, procesandoId, onCantidad, onLiberar }: ComandaCocinaCardProps) {
  const completa = comanda.estado === 'Lista';

  return (
    <article className={`card kitchen-order-card border-0 shadow-sm ${completa ? 'complete' : ''}`}>
      <header className="card-header bg-white border-0 pt-3 px-3">
        <div className="d-flex justify-content-between align-items-start">
          <div>
            <div className="small text-secondary text-uppercase">Mesa</div>
            <div className="kitchen-table-number">{comanda.mesaNumero}</div>
          </div>
          <div className="text-end">
            <span className={`badge ${completa ? 'text-bg-success' : comanda.estado === 'ParcialmenteLista' ? 'text-bg-warning' : 'text-bg-danger'}`}>
              {formatearEstado(comanda.estado)}
            </span>
            <div className="small text-secondary mt-2">Mesero: {comanda.mesero}</div>
            <div className="small text-secondary">{formatearHora(comanda.fechaHoraEnvioCocinaUtc)}</div>
          </div>
        </div>
        <div className="progress kitchen-progress mt-3" role="progressbar" aria-label="Preparación">
          <div className="progress-bar" style={{ width: `${porcentajePreparado(comanda)}%` }} />
        </div>
        <div className="small text-secondary mt-1">
          {comanda.totalPreparados} preparados de {comanda.totalProductos} · {comanda.totalPendientes} pendientes
        </div>
      </header>
      <div className="card-body px-3 pt-3">
        {comanda.comensales.map((comensal) => (
          <section key={comensal.id} className="mb-3 last-no-margin">
            <h2 className="h6 text-uppercase text-secondary mb-2">Comensal {comensal.numero}</h2>
            <div className="d-grid gap-2">
              {comensal.partidas.map((detalle) => (
                <PartidaCocinaRow
                  key={detalle.id}
                  detalle={detalle}
                  completa={completa}
                  cantidad={cantidades[detalle.id] ?? detalle.cantidadPendiente}
                  procesando={procesandoId === detalle.id}
                  onCantidad={onCantidad}
                  onLiberar={() => onLiberar(comanda, detalle)}
                />
              ))}
            </div>
          </section>
        ))}
      </div>
    </article>
  );
}

interface PartidaCocinaRowProps {
  detalle: PartidaCocina;
  completa: boolean;
  cantidad: number;
  procesando: boolean;
  onCantidad: (detalle: PartidaCocina, cantidad: number) => void;
  onLiberar: () => Promise<void>;
}

function PartidaCocinaRow({ detalle, completa, cantidad, procesando, onCantidad, onLiberar }: PartidaCocinaRowProps) {
  const terminada = detalle.cantidadPendiente === 0;

  return (
    <div className={`kitchen-line-item ${terminada ? 'prepared' : ''}`}>
      <div className="d-flex justify-content-between align-items-start gap-2">
        <div>
          <div className="fw-semibold">{detalle.producto}</div>
          <div className="small text-secondary">
            Pedido: {detalle.cantidad} · Listo: {detalle.cantidadPreparada} · Pendiente: {detalle.cantidadPendiente}
          </div>
        </div>
        {terminada && <i className="bi bi-check-circle-fill text-success fs-5" aria-label="Completo" />}
      </div>

      {detalle.entregasParciales.length > 0 && (
        <div className="d-flex flex-wrap gap-1 mt-2">
          {detalle.entregasParciales.map((entrega) => (
            <span className="partial-chip" key={entrega.id}>
              +{entrega.cantidad} · {formatearHora(entrega.fechaHoraListaUtc)}
            </span>
          ))}
        </div>
      )}

      {!completa && !terminada && (
        <div className="release-control mt-3">
          <input
            className="form-control form-control-lg"
            aria-label={`Cantidad lista de ${detalle.producto}`}
            type="number"
            inputMode="numeric"
            min={1}
            max={detalle.cantidadPendiente}
            value={cantidad}
            onChange={(event) => onCantidad(detalle, Number(event.target.value))}
          />
          <Button appearance="primary" size="large" disabled={procesando} onClick={() => void onLiberar()}>
            {procesando ? <Spinner size="tiny" /> : 'Liberar'}
          </Button>
        </div>
      )}
    </div>
  );
}

function inicializarCantidades(comandas: ComandaCocina[], actuales: CantidadesLiberacion): CantidadesLiberacion {
  const resultado = { ...actuales };
  for (const comanda of comandas) {
    for (const comensal of comanda.comensales) {
      for (const detalle of comensal.partidas) {
        if (detalle.cantidadPendiente > 0) {
          resultado[detalle.id] = Math.min(
            detalle.cantidadPendiente,
            Math.max(1, resultado[detalle.id] ?? detalle.cantidadPendiente)
          );
        } else {
          delete resultado[detalle.id];
        }
      }
    }
  }
  return resultado;
}

function porcentajePreparado(comanda: ComandaCocina) {
  return comanda.totalProductos === 0 ? 0 : Math.round((comanda.totalPreparados / comanda.totalProductos) * 100);
}

function formatearHora(value: string | null) {
  if (!value) return '';
  return new Date(value).toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' });
}

function formatearEstado(estado: string) {
  switch (estado) {
    case 'EnPreparacion': return 'En preparación';
    case 'ParcialmenteLista': return 'Parcialmente lista';
    case 'Lista': return 'Lista';
    default: return estado;
  }
}
