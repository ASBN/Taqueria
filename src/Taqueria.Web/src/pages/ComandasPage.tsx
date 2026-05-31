import { useCallback, useEffect, useMemo, useState } from 'react';
import { Button, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components';
import type { Comanda, Comensal, ComandaDetalle, EntregaParcialMesero, Mesa, Producto } from '../models/api';
import { apiClient } from '../services/apiClient';
import { useAuth } from '../features/auth/AuthContext';
import { currency } from '../utils/format';

export function ComandasPage() {
  const { usuario } = useAuth();
  const [comandas, setComandas] = useState<Comanda[]>([]);
  const [mesas, setMesas] = useState<Mesa[]>([]);
  const [productos, setProductos] = useState<Producto[]>([]);
  const [comandaSeleccionadaId, setComandaSeleccionadaId] = useState<number | null>(null);
  const [comensalSeleccionadoId, setComensalSeleccionadoId] = useState<number | null>(null);
  const [cantidad, setCantidad] = useState(1);
  const [cargando, setCargando] = useState(true);
  const [procesando, setProcesando] = useState(false);
  const [mensaje, setMensaje] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [mostrarCancelacion, setMostrarCancelacion] = useState(false);
  const [motivoCancelacion, setMotivoCancelacion] = useState('');

  const comanda = comandas.find((item) => item.id === comandaSeleccionadaId) ?? null;
  const comensal = comanda?.comensales.find((item) => item.id === comensalSeleccionadoId)
    ?? comanda?.comensales[0]
    ?? null;

  const mesasOcupadas = new Set(comandas.map((item) => item.mesaId));
  const mesasLibres = mesas.filter((mesa) => mesa.activa && !mesasOcupadas.has(mesa.id));
  const productosDisponibles = productos.filter((producto) => producto.activo && producto.precioVigente !== null);
  const categorias = useMemo(
    () => Array.from(new Set(productosDisponibles.map((producto) => producto.categoriaNombre))),
    [productosDisponibles]
  );
  const esResponsable = usuario?.rol === 'Administrador' || comanda?.usuarioMeseroId === usuario?.id;
  const puedeEditar = comanda?.estado === 'Abierta' && esResponsable;
  const puedeEnviarACobro = comanda?.estado === 'Entregada' && esResponsable;
  const puedeCancelar = Boolean(comanda && esResponsable
    && (comanda.estado === 'Abierta' || usuario?.rol === 'Administrador')
    && comanda.estado !== 'Cobrada'
    && comanda.estado !== 'Cancelada');
  const totalEntregasListas = comanda?.comensales
    .flatMap((item) => item.detalles)
    .flatMap((detalle) => detalle.entregasParciales)
    .filter((entrega) => entrega.estado === 'ListaParaEntregar').length ?? 0;

  const refrescarComandas = useCallback(async () => {
    const response = await apiClient.get<Comanda[]>('/api/comandas/activas');
    setComandas(response);
    setComandaSeleccionadaId((seleccionada) => {
      if (seleccionada && !response.some((item) => item.id === seleccionada)) {
        setComensalSeleccionadoId(null);
        return null;
      }
      return seleccionada;
    });
  }, []);

  async function cargar() {
    setCargando(true);
    setError(null);
    try {
      const [comandasResponse, mesasResponse, productosResponse] = await Promise.all([
        apiClient.get<Comanda[]>('/api/comandas/activas'),
        apiClient.get<Mesa[]>('/api/mesas'),
        apiClient.get<Producto[]>('/api/productos')
      ]);
      setComandas(comandasResponse);
      setMesas(mesasResponse);
      setProductos(productosResponse);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible cargar las comandas.');
    } finally {
      setCargando(false);
    }
  }

  useEffect(() => {
    void cargar();
  }, []);

  useEffect(() => {
    const interval = window.setInterval(() => {
      void refrescarComandas().catch(() => undefined);
    }, 15000);
    return () => window.clearInterval(interval);
  }, [refrescarComandas]);

  async function ejecutar(operacion: () => Promise<Comanda>, textoConfirmacion: string) {
    setProcesando(true);
    setError(null);
    setMensaje(null);
    try {
      const actualizada = await operacion();
      setComandaSeleccionadaId(actualizada.id);
      setComandas((actuales) => {
        const existe = actuales.some((item) => item.id === actualizada.id);
        return existe
          ? actuales.map((item) => item.id === actualizada.id ? actualizada : item)
          : [actualizada, ...actuales];
      });
      setMensaje(textoConfirmacion);
      return actualizada;
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible completar la operación.');
      return null;
    } finally {
      setProcesando(false);
    }
  }

  async function abrirMesa(mesa: Mesa) {
    const actualizada = await ejecutar(
      () => apiClient.post<Comanda>('/api/comandas', { mesaId: mesa.id }),
      `Mesa ${mesa.numero} abierta. Capture el pedido del comensal 1.`
    );
    setComensalSeleccionadoId(actualizada?.comensales[0]?.id ?? null);
  }

  function seleccionarComanda(item: Comanda) {
    setComandaSeleccionadaId(item.id);
    setComensalSeleccionadoId(item.comensales[0]?.id ?? null);
    setMensaje(null);
    setError(null);
    setMostrarCancelacion(false);
    setMotivoCancelacion('');
  }

  async function agregarComensal() {
    if (!comanda) return;
    const numero = Math.max(0, ...comanda.comensales.map((item) => item.numero)) + 1;
    const actualizada = await ejecutar(
      () => apiClient.post<Comanda>(`/api/comandas/${comanda.id}/comensales`, { numero }),
      `Comensal ${numero} agregado.`
    );
    const nuevo = actualizada?.comensales.find((item) => item.numero === numero);
    setComensalSeleccionadoId(nuevo?.id ?? null);
  }

  async function agregarProducto(producto: Producto) {
    if (!comanda || !comensal) return;
    await ejecutar(
      () => apiClient.post<Comanda>(`/api/comandas/${comanda.id}/comensales/${comensal.id}/partidas`, {
        productoId: producto.id,
        cantidad
      }),
      `${cantidad} × ${producto.nombre} agregado a comensal ${comensal.numero}.`
    );
    setCantidad(1);
  }

  async function quitarPartida(comensalId: number, detalleId: number) {
    if (!comanda) return;
    await ejecutar(
      () => apiClient.delete<Comanda>(`/api/comandas/${comanda.id}/comensales/${comensalId}/partidas/${detalleId}`),
      'Partida retirada de la comanda.'
    );
  }

  async function enviarACocina() {
    if (!comanda || !window.confirm('¿Enviar la comanda a cocina? Después de enviarla ya no puede modificarse en esta fase.')) {
      return;
    }

    await ejecutar(
      () => apiClient.post<Comanda>(`/api/comandas/${comanda.id}/enviar-cocina`, {}),
      `Comanda de mesa ${comanda.mesaNumero} enviada a cocina.`
    );
  }

  async function confirmarEntrega(entrega: EntregaParcialMesero) {
    if (!comanda) return;
    await ejecutar(
      () => apiClient.post<Comanda>(`/api/comandas/${comanda.id}/entregas-parciales/${entrega.id}/confirmar`, {}),
      `${entrega.cantidad} producto(s) entregados a la mesa ${comanda.mesaNumero}.`
    );
  }

  async function enviarACobro() {
    if (!comanda || !window.confirm(`¿Enviar la cuenta de ${currency(comanda.total)} a Caja? La mesa permanecerá ocupada hasta registrar el cobro.`)) {
      return;
    }

    await ejecutar(
      () => apiClient.post<Comanda>(`/api/comandas/${comanda.id}/enviar-cobro`, {}),
      `Cuenta de mesa ${comanda.mesaNumero} enviada a Caja.`
    );
  }

  async function cancelarComanda() {
    if (!comanda || !motivoCancelacion.trim()) {
      setError('Indique el motivo de cancelación.');
      return;
    }

    if (!window.confirm(`¿Cancelar la comanda de mesa ${comanda.mesaNumero}? Esta acción liberará la mesa y, si existen productos preparados no entregados, registrará la merma correspondiente.`)) {
      return;
    }

    setProcesando(true);
    setError(null);
    setMensaje(null);
    try {
      await apiClient.post<Comanda>(`/api/comandas/${comanda.id}/cancelar`, { motivo: motivoCancelacion.trim() });
      setComandas((actuales) => actuales.filter((item) => item.id !== comanda.id));
      setComandaSeleccionadaId(null);
      setComensalSeleccionadoId(null);
      setMostrarCancelacion(false);
      setMotivoCancelacion('');
      setMensaje(`Comanda de mesa ${comanda.mesaNumero} cancelada. La mesa quedó disponible.`);
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible cancelar la comanda.');
    } finally {
      setProcesando(false);
    }
  }

  if (cargando) {
    return <div className="text-center py-5"><Spinner label="Cargando operación..." /></div>;
  }

  return (
    <div className="mx-auto page-width">
      <div className="d-flex align-items-center justify-content-between mb-3">
        <div>
          <h1 className="h4 mb-1">Comandas</h1>
          <p className="text-secondary small mb-0">Pedido, entrega a mesa y envío a caja</p>
        </div>
        <div className="d-flex gap-2">
          <Button appearance="secondary" onClick={() => void refrescarComandas()}>Actualizar</Button>
          {comanda && (
            <Button appearance="secondary" onClick={() => setComandaSeleccionadaId(null)}>Mesas</Button>
          )}
        </div>
      </div>

      {mensaje && (
        <MessageBar intent="success" className="mb-3"><MessageBarBody>{mensaje}</MessageBarBody></MessageBar>
      )}
      {error && (
        <MessageBar intent="error" className="mb-3"><MessageBarBody>{error}</MessageBarBody></MessageBar>
      )}

      {!comanda ? (
        <SeleccionMesa
          comandas={comandas}
          mesasLibres={mesasLibres}
          procesando={procesando}
          onAbrir={abrirMesa}
          onSeleccionar={seleccionarComanda}
        />
      ) : (
        <section>
          <header className="comanda-header card border-0 shadow-sm mb-3">
            <div className="card-body d-flex justify-content-between align-items-center">
              <div>
                <div className="small text-uppercase text-secondary">Mesa {comanda.mesaNumero}</div>
                <div className="fw-semibold">{comanda.folio}</div>
                <span className={`badge mt-2 ${badgeEstado(comanda.estado)}`}>
                  {formatearEstado(comanda.estado)}
                </span>
              </div>
              <div className="text-end">
                <div className="small text-secondary">Cuenta</div>
                <div className="fs-3 fw-bold text-brand">{currency(comanda.total)}</div>
                {totalEntregasListas > 0 && <span className="badge text-bg-danger">{totalEntregasListas} lote(s) listo(s)</span>}
              </div>
            </div>
          </header>

          <div className="d-flex gap-2 overflow-x-auto pb-2 mb-2">
            {comanda.comensales.map((item) => (
              <button
                className={`btn comensal-pill ${comensal?.id === item.id ? 'active' : ''}`}
                type="button"
                key={item.id}
                onClick={() => setComensalSeleccionadoId(item.id)}
              >
                <div>Comensal {item.numero}</div>
                <small>{currency(item.subtotal)}</small>
              </button>
            ))}
            {puedeEditar && (
              <button className="btn add-comensal-button" type="button" disabled={procesando} onClick={() => void agregarComensal()}>
                <i className="bi bi-person-plus fs-5" />
                <small>Agregar</small>
              </button>
            )}
          </div>

          {comensal && (
            <DetalleComensal
              comensal={comensal}
              editable={Boolean(puedeEditar)}
              puedeEntregar={Boolean(esResponsable)}
              procesando={procesando}
              onQuitar={quitarPartida}
              onConfirmarEntrega={confirmarEntrega}
            />
          )}

          {puedeEditar ? (
            <>
              <div className="card border-0 shadow-sm mt-3 mb-3">
                <div className="card-body">
                  <div className="d-flex align-items-center justify-content-between gap-3">
                    <h2 className="h6 mb-0">Cantidad a agregar</h2>
                    <div className="quantity-control">
                      <button className="btn btn-outline-secondary" type="button" onClick={() => setCantidad(Math.max(1, cantidad - 1))}>−</button>
                      <span>{cantidad}</span>
                      <button className="btn btn-outline-secondary" type="button" onClick={() => setCantidad(cantidad + 1)}>+</button>
                    </div>
                  </div>
                </div>
              </div>

              {categorias.map((categoria) => (
                <section key={categoria} className="mb-3">
                  <h2 className="h6 text-secondary text-uppercase">{categoria}</h2>
                  <div className="row g-2">
                    {productosDisponibles.filter((producto) => producto.categoriaNombre === categoria).map((producto) => (
                      <div className="col-6" key={producto.id}>
                        <button className="btn product-order-button w-100" type="button" disabled={procesando} onClick={() => void agregarProducto(producto)}>
                          <span>{producto.nombre}</span>
                          <strong>{currency(producto.precioVigente!)}</strong>
                        </button>
                      </div>
                    ))}
                  </div>
                </section>
              ))}

              <Button
                appearance="primary"
                size="large"
                className="w-100 mt-2"
                disabled={procesando || comanda.total <= 0}
                onClick={() => void enviarACocina()}
              >
                Enviar a cocina
              </Button>
            </>
          ) : puedeEnviarACobro ? (
            <div className="card border-0 shadow-sm mt-3">
              <div className="card-body">
                <h2 className="h6">Cuenta lista para cobrar</h2>
                <p className="small text-secondary">Toda la orden fue entregada. Envía la cuenta a caja; la mesa quedará ocupada hasta que el cobro se registre.</p>
                <Button appearance="primary" size="large" className="w-100" disabled={procesando} onClick={() => void enviarACobro()}>
                  Enviar cuenta a Caja · {currency(comanda.total)}
                </Button>
              </div>
            </div>
          ) : (
            <EstadoAtencion estado={comanda.estado} entregasListas={totalEntregasListas} />
          )}

          {puedeCancelar && (
            <div className="card border-0 shadow-sm mt-3 cancellation-card">
              <div className="card-body">
                {!mostrarCancelacion ? (
                  <Button appearance="secondary" className="w-100" onClick={() => setMostrarCancelacion(true)}>
                    Cancelar comanda
                  </Button>
                ) : (
                  <>
                    <h2 className="h6 text-danger">Cancelar comanda</h2>
                    <p className="small text-secondary">
                      El mesero puede cancelar antes de cocina. Administración puede cancelar una orden preparada no entregada; las porciones preparadas se registrarán automáticamente como merma.
                    </p>
                    <textarea
                      className="form-control mb-2"
                      rows={3}
                      maxLength={300}
                      placeholder="Motivo obligatorio"
                      value={motivoCancelacion}
                      onChange={(event) => setMotivoCancelacion(event.target.value)}
                    />
                    <div className="d-flex gap-2">
                      <Button appearance="primary" disabled={procesando || !motivoCancelacion.trim()} onClick={() => void cancelarComanda()}>Confirmar cancelación</Button>
                      <Button appearance="secondary" onClick={() => { setMostrarCancelacion(false); setMotivoCancelacion(''); }}>Volver</Button>
                    </div>
                  </>
                )}
              </div>
            </div>
          )}
        </section>
      )}
    </div>
  );
}

interface SeleccionMesaProps {
  comandas: Comanda[];
  mesasLibres: Mesa[];
  procesando: boolean;
  onAbrir: (mesa: Mesa) => Promise<void>;
  onSeleccionar: (comanda: Comanda) => void;
}

function SeleccionMesa({ comandas, mesasLibres, procesando, onAbrir, onSeleccionar }: SeleccionMesaProps) {
  return (
    <>
      {comandas.length > 0 && (
        <section className="mb-4">
          <h2 className="h6 text-secondary text-uppercase">En servicio</h2>
          <div className="row g-2">
            {comandas.map((comanda) => (
              <div className="col-6" key={comanda.id}>
                <button className="card service-table-card border-0 shadow-sm w-100 text-start" type="button" onClick={() => onSeleccionar(comanda)}>
                  <div className="card-body p-3">
                    <div className="d-flex justify-content-between gap-1">
                      <strong>Mesa {comanda.mesaNumero}</strong>
                      <span className="badge text-bg-light">{formatearEstado(comanda.estado)}</span>
                    </div>
                    <div className="small text-secondary mt-2">{comanda.comensales.length} comensal(es)</div>
                    <div className="fw-bold mt-1 text-brand">{currency(comanda.total)}</div>
                  </div>
                </button>
              </div>
            ))}
          </div>
        </section>
      )}

      <section>
        <h2 className="h6 text-secondary text-uppercase">Abrir mesa libre</h2>
        {mesasLibres.length === 0 ? (
          <div className="card border-0 shadow-sm"><div className="card-body text-secondary">No hay mesas libres activas.</div></div>
        ) : (
          <div className="row g-2">
            {mesasLibres.map((mesa) => (
              <div className="col-4" key={mesa.id}>
                <button className="btn free-table-button w-100" type="button" disabled={procesando} onClick={() => void onAbrir(mesa)}>
                  <strong>{mesa.numero}</strong>
                  <small>{mesa.capacidad} pers.</small>
                </button>
              </div>
            ))}
          </div>
        )}
      </section>
    </>
  );
}

interface DetalleComensalProps {
  comensal: Comensal;
  editable: boolean;
  puedeEntregar: boolean;
  procesando: boolean;
  onQuitar: (comensalId: number, detalleId: number) => Promise<void>;
  onConfirmarEntrega: (entrega: EntregaParcialMesero) => Promise<void>;
}

function DetalleComensal({ comensal, editable, puedeEntregar, procesando, onQuitar, onConfirmarEntrega }: DetalleComensalProps) {
  return (
    <article className="card border-0 shadow-sm mt-2">
      <div className="card-body">
        <h2 className="h6">Pedido del comensal {comensal.numero}</h2>
        {comensal.detalles.length === 0 ? (
          <p className="small text-secondary mb-0">Seleccione productos para comenzar su pedido.</p>
        ) : (
          <div className="d-grid gap-2">
            {comensal.detalles.map((detalle) => (
              <DetallePartida
                key={detalle.id}
                detalle={detalle}
                comensalId={comensal.id}
                editable={editable}
                puedeEntregar={puedeEntregar}
                procesando={procesando}
                onQuitar={onQuitar}
                onConfirmarEntrega={onConfirmarEntrega}
              />
            ))}
          </div>
        )}
      </div>
    </article>
  );
}

interface DetallePartidaProps {
  detalle: ComandaDetalle;
  comensalId: number;
  editable: boolean;
  puedeEntregar: boolean;
  procesando: boolean;
  onQuitar: (comensalId: number, detalleId: number) => Promise<void>;
  onConfirmarEntrega: (entrega: EntregaParcialMesero) => Promise<void>;
}

function DetallePartida({ detalle, comensalId, editable, puedeEntregar, procesando, onQuitar, onConfirmarEntrega }: DetallePartidaProps) {
  return (
    <div className="line-item delivery-line-item">
      <div className="d-flex justify-content-between align-items-start gap-2">
        <div>
          <span className="fw-semibold">{detalle.cantidad} × {detalle.nombreProductoHistorico}</span>
          <div className="small text-secondary">{currency(detalle.precioUnitarioHistorico)} c/u · Entregado {detalle.cantidadEntregada}/{detalle.cantidad}</div>
        </div>
        <div className="d-flex align-items-center gap-2">
          <strong>{currency(detalle.subtotal)}</strong>
          {editable && (
            <button className="btn btn-outline-danger btn-sm" type="button" aria-label="Quitar partida" onClick={() => void onQuitar(comensalId, detalle.id)}>
              <i className="bi bi-trash" />
            </button>
          )}
        </div>
      </div>
      {detalle.entregasParciales.length > 0 && (
        <div className="d-grid gap-2 mt-2">
          {detalle.entregasParciales.map((entrega) => (
            <div className={`waiter-delivery-chip ${entrega.estado === 'Entregada' ? 'delivered' : 'ready'}`} key={entrega.id}>
              <div>
                <strong>{entrega.cantidad}</strong> listos · {formatearHora(entrega.fechaHoraListaUtc)}
                <div className="small">{entrega.estado === 'Entregada' ? `Entregado ${formatearHora(entrega.fechaHoraEntregaUtc)}` : 'Listo para llevar a mesa'}</div>
              </div>
              {entrega.estado === 'ListaParaEntregar' && puedeEntregar && (
                <Button appearance="primary" size="small" disabled={procesando} onClick={() => void onConfirmarEntrega(entrega)}>
                  Entregar
                </Button>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}

function EstadoAtencion({ estado, entregasListas }: { estado: string; entregasListas: number }) {
  if (estado === 'PendienteCobro') {
    return <div className="alert alert-warning mt-3 mb-0">Cuenta enviada a Caja. La mesa permanece ocupada hasta registrar el pago.</div>;
  }

  if (entregasListas > 0) {
    return <div className="alert alert-danger mt-3 mb-0">Hay productos listos: confirma su entrega a la mesa en las partidas superiores.</div>;
  }

  return <div className="alert alert-info mt-3 mb-0">La comanda está en cocina. Esta pantalla se actualiza automáticamente cada 15 segundos.</div>;
}

function formatearEstado(estado: string) {
  switch (estado) {
    case 'EnPreparacion': return 'En preparación';
    case 'ParcialmenteLista': return 'Parcialmente lista';
    case 'PendienteCobro': return 'En Caja';
    default: return estado;
  }
}

function badgeEstado(estado: string) {
  switch (estado) {
    case 'Abierta': return 'text-bg-warning';
    case 'PendienteCobro': return 'text-bg-warning';
    case 'Entregada': return 'text-bg-primary';
    case 'Lista': return 'text-bg-success';
    default: return 'text-bg-info';
  }
}

function formatearHora(value: string | null) {
  if (!value) return '';
  return new Date(value).toLocaleTimeString('es-MX', { hour: '2-digit', minute: '2-digit' });
}
