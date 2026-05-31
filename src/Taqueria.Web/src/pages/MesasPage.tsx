import { useEffect, useState, type FormEvent } from 'react';
import { Button, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components';
import type { Mesa } from '../models/api';
import { apiClient } from '../services/apiClient';

interface FormMesa {
  id?: number;
  numero: number;
  capacidad: number;
  activa: boolean;
}

const nuevaMesa: FormMesa = { numero: 1, capacidad: 4, activa: true };

export function MesasPage() {
  const [mesas, setMesas] = useState<Mesa[]>([]);
  const [form, setForm] = useState<FormMesa>(nuevaMesa);
  const [mostrarForm, setMostrarForm] = useState(false);
  const [cargando, setCargando] = useState(true);
  const [mensaje, setMensaje] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function cargar() {
    setCargando(true);
    try {
      setMesas(await apiClient.get<Mesa[]>('/api/mesas'));
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'Error al cargar mesas.');
    } finally {
      setCargando(false);
    }
  }

  useEffect(() => {
    void cargar();
  }, []);

  function abrirNueva() {
    const siguienteNumero = mesas.length ? Math.max(...mesas.map((mesa) => mesa.numero)) + 1 : 1;
    setForm({ ...nuevaMesa, numero: siguienteNumero });
    setMostrarForm(true);
  }

  function editar(mesa: Mesa) {
    setForm({ ...mesa });
    setMostrarForm(true);
  }

  async function guardar(event: FormEvent) {
    event.preventDefault();
    setError(null);
    try {
      if (form.id) {
        await apiClient.put<Mesa>(`/api/mesas/${form.id}`, form);
        setMensaje(`Mesa ${form.numero} actualizada.`);
      } else {
        await apiClient.post<Mesa>('/api/mesas', { numero: form.numero, capacidad: form.capacidad });
        setMensaje(`Mesa ${form.numero} creada.`);
      }
      setMostrarForm(false);
      await cargar();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible guardar la mesa.');
    }
  }

  async function eliminar(mesa: Mesa) {
    if (!window.confirm(`¿Retirar la mesa ${mesa.numero}? Si tiene histórico sólo quedará inactiva.`)) {
      return;
    }

    try {
      await apiClient.delete(`/api/mesas/${mesa.id}`);
      setMensaje(`Mesa ${mesa.numero} retirada.`);
      await cargar();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible retirar la mesa.');
    }
  }

  return (
    <div className="mx-auto page-width">
      <div className="d-flex align-items-center justify-content-between mb-3">
        <div>
          <h1 className="h4 mb-1">Mesas</h1>
          <p className="text-secondary small mb-0">Catálogo del salón</p>
        </div>
        <Button appearance="primary" size="large" onClick={abrirNueva}>Nueva</Button>
      </div>

      {mensaje && (
        <MessageBar intent="success" className="mb-3">
          <MessageBarBody>{mensaje}</MessageBarBody>
        </MessageBar>
      )}
      {error && (
        <MessageBar intent="error" className="mb-3">
          <MessageBarBody>{error}</MessageBarBody>
        </MessageBar>
      )}

      {mostrarForm && (
        <form className="card border-0 shadow-sm mb-3" onSubmit={guardar}>
          <div className="card-body">
            <h2 className="h6">{form.id ? 'Editar mesa' : 'Nueva mesa'}</h2>
            <div className="row g-2 mb-3">
              <div className="col-6">
                <label className="form-label">Número</label>
                <input className="form-control form-control-lg" type="number" min="1" required value={form.numero}
                  onChange={(event) => setForm({ ...form, numero: Number(event.target.value) })} />
              </div>
              <div className="col-6">
                <label className="form-label">Capacidad</label>
                <input className="form-control form-control-lg" type="number" min="1" required value={form.capacidad}
                  onChange={(event) => setForm({ ...form, capacidad: Number(event.target.value) })} />
              </div>
            </div>
            {form.id && (
              <div className="form-check form-switch mb-3">
                <input className="form-check-input" type="checkbox" checked={form.activa}
                  onChange={(event) => setForm({ ...form, activa: event.target.checked })} id="mesaActiva" />
                <label className="form-check-label" htmlFor="mesaActiva">Mesa activa</label>
              </div>
            )}
            <div className="d-flex gap-2">
              <Button appearance="primary" type="submit">Guardar</Button>
              <Button appearance="secondary" type="button" onClick={() => setMostrarForm(false)}>Cancelar</Button>
            </div>
          </div>
        </form>
      )}

      {cargando ? (
        <div className="text-center py-5"><Spinner label="Cargando mesas..." /></div>
      ) : (
        <div className="row g-3">
          {mesas.map((mesa) => (
            <div key={mesa.id} className="col-6 col-sm-4 col-md-3">
              <article className={`card mesa-card h-100 border-0 shadow-sm ${!mesa.activa ? 'inactive-card' : ''}`}>
                <div className="card-body text-center">
                  <div className="mesa-number">{mesa.numero}</div>
                  <div className="small text-secondary mb-3">
                    <i className="bi bi-people me-1" />{mesa.capacidad} personas
                  </div>
                  {!mesa.activa && <span className="badge text-bg-secondary mb-2">Inactiva</span>}
                  <div className="d-flex justify-content-center gap-2">
                    <button className="btn btn-outline-secondary btn-sm" type="button" onClick={() => editar(mesa)}>
                      <i className="bi bi-pencil" />
                    </button>
                    <button className="btn btn-outline-danger btn-sm" type="button" onClick={() => void eliminar(mesa)}>
                      <i className="bi bi-trash" />
                    </button>
                  </div>
                </div>
              </article>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
