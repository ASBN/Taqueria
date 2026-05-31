import { useEffect, useState, type FormEvent } from 'react';
import { Button, Field, Input, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components';
import type { Usuario } from '../models/api';
import { apiClient } from '../services/apiClient';

const roles = ['Administrador', 'Mesero', 'Cocina', 'Caja'];

interface FormularioUsuario {
  id: number | null;
  nombre: string;
  nombreUsuario: string;
  rol: string;
  activo: boolean;
  passwordTemporal: string;
  confirmarPasswordTemporal: string;
}

const formularioInicial: FormularioUsuario = {
  id: null,
  nombre: '',
  nombreUsuario: '',
  rol: 'Mesero',
  activo: true,
  passwordTemporal: '',
  confirmarPasswordTemporal: ''
};

export function UsuariosPage() {
  const [usuarios, setUsuarios] = useState<Usuario[]>([]);
  const [formulario, setFormulario] = useState<FormularioUsuario>(formularioInicial);
  const [cargando, setCargando] = useState(true);
  const [procesando, setProcesando] = useState(false);
  const [mensaje, setMensaje] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);
  const editando = formulario.id !== null;

  useEffect(() => { void cargar(); }, []);

  async function cargar() {
    setCargando(true);
    try {
      setUsuarios(await apiClient.get<Usuario[]>('/api/usuarios'));
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible cargar usuarios.');
    } finally {
      setCargando(false);
    }
  }

  function editar(usuario: Usuario) {
    setFormulario({
      id: usuario.id,
      nombre: usuario.nombre,
      nombreUsuario: usuario.nombreUsuario,
      rol: usuario.rol,
      activo: usuario.activo,
      passwordTemporal: '',
      confirmarPasswordTemporal: ''
    });
    setMensaje(null);
    setError(null);
  }

  async function guardar(event: FormEvent) {
    event.preventDefault();
    setProcesando(true);
    setMensaje(null);
    setError(null);
    try {
      if (editando) {
        await apiClient.put<Usuario>(`/api/usuarios/${formulario.id}`, {
          nombre: formulario.nombre,
          nombreUsuario: formulario.nombreUsuario,
          rol: formulario.rol,
          activo: formulario.activo
        });
        setMensaje('Usuario actualizado.');
      } else {
        await apiClient.post<Usuario>('/api/usuarios', {
          nombre: formulario.nombre,
          nombreUsuario: formulario.nombreUsuario,
          rol: formulario.rol,
          passwordTemporal: formulario.passwordTemporal,
          confirmarPasswordTemporal: formulario.confirmarPasswordTemporal
        });
        setMensaje('Usuario creado. Deberá cambiar su contraseña temporal al iniciar sesión.');
      }
      setFormulario(formularioInicial);
      await cargar();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible guardar el usuario.');
    } finally {
      setProcesando(false);
    }
  }

  async function restablecerPassword(usuario: Usuario) {
    const passwordTemporal = window.prompt(`Nueva contraseña temporal para ${usuario.nombre}:`);
    if (!passwordTemporal) return;
    const confirmacion = window.prompt('Confirme la contraseña temporal:');
    if (confirmacion === null) return;
    setProcesando(true);
    setError(null);
    try {
      await apiClient.post<Usuario>(`/api/usuarios/${usuario.id}/restablecer-password`, {
        passwordTemporal,
        confirmarPasswordTemporal: confirmacion
      });
      setMensaje(`Contraseña temporal de ${usuario.nombre} restablecida.`);
      await cargar();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible restablecer la contraseña.');
    } finally {
      setProcesando(false);
    }
  }

  async function desactivar(usuario: Usuario) {
    if (!window.confirm(`¿Desactivar el acceso de ${usuario.nombre}? Su histórico se conservará.`)) return;
    setProcesando(true);
    setError(null);
    try {
      await apiClient.delete(`/api/usuarios/${usuario.id}`);
      setMensaje(`Acceso de ${usuario.nombre} desactivado.`);
      await cargar();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible desactivar el usuario.');
    } finally {
      setProcesando(false);
    }
  }

  if (cargando) return <div className="text-center py-5"><Spinner label="Cargando usuarios..." /></div>;

  return (
    <div className="mx-auto page-width">
      <h1 className="h4 mb-1">Usuarios</h1>
      <p className="text-secondary small mb-3">Accesos operativos, roles y contraseñas temporales</p>
      {mensaje && <MessageBar intent="success" className="mb-3"><MessageBarBody>{mensaje}</MessageBarBody></MessageBar>}
      {error && <MessageBar intent="error" className="mb-3"><MessageBarBody>{error}</MessageBarBody></MessageBar>}

      <section className="card border-0 shadow-sm mb-3">
        <div className="card-body">
          <h2 className="h6 mb-3">{editando ? 'Editar usuario' : 'Nuevo usuario'}</h2>
          <form className="row g-3" onSubmit={guardar}>
            <div className="col-12"><Field label="Nombre" required><Input value={formulario.nombre} onChange={(_, d) => setFormulario({ ...formulario, nombre: d.value })} /></Field></div>
            <div className="col-12 col-sm-6"><Field label="Usuario" required><Input value={formulario.nombreUsuario} onChange={(_, d) => setFormulario({ ...formulario, nombreUsuario: d.value })} /></Field></div>
            <div className="col-12 col-sm-6">
              <label className="form-label small fw-semibold">Rol</label>
              <select className="form-select" value={formulario.rol} onChange={(e) => setFormulario({ ...formulario, rol: e.target.value })}>
                {roles.map((rol) => <option key={rol}>{rol}</option>)}
              </select>
            </div>
            {!editando && <>
              <div className="col-12 col-sm-6"><Field label="Contraseña temporal" required><Input type="password" value={formulario.passwordTemporal} onChange={(_, d) => setFormulario({ ...formulario, passwordTemporal: d.value })} /></Field></div>
              <div className="col-12 col-sm-6"><Field label="Confirmar contraseña" required><Input type="password" value={formulario.confirmarPasswordTemporal} onChange={(_, d) => setFormulario({ ...formulario, confirmarPasswordTemporal: d.value })} /></Field></div>
            </>}
            {editando && <div className="col-12 form-check ms-2"><input className="form-check-input" type="checkbox" checked={formulario.activo} onChange={(e) => setFormulario({ ...formulario, activo: e.target.checked })} id="activo" /><label className="form-check-label" htmlFor="activo">Usuario activo</label></div>}
            <div className="col-12 d-flex gap-2">
              <Button appearance="primary" type="submit" disabled={procesando}>{editando ? 'Actualizar' : 'Crear usuario'}</Button>
              {editando && <Button appearance="secondary" type="button" onClick={() => setFormulario(formularioInicial)}>Cancelar</Button>}
            </div>
          </form>
        </div>
      </section>

      <div className="d-grid gap-2">
        {usuarios.map((usuario) => (
          <article key={usuario.id} className={`card border-0 shadow-sm user-card ${usuario.activo ? '' : 'inactive-card'}`}>
            <div className="card-body">
              <div className="d-flex justify-content-between gap-2 mb-2">
                <div><div className="fw-semibold">{usuario.nombre}</div><div className="small text-secondary">@{usuario.nombreUsuario} · {usuario.rol}</div></div>
                <span className={`badge ${usuario.activo ? 'text-bg-success' : 'text-bg-secondary'}`}>{usuario.activo ? 'Activo' : 'Inactivo'}</span>
              </div>
              {usuario.debeCambiarPassword && <div className="small text-warning-emphasis mb-2"><i className="bi bi-key me-1" />Contraseña temporal pendiente de cambio</div>}
              <div className="d-flex flex-wrap gap-2">
                <Button appearance="secondary" size="small" onClick={() => editar(usuario)}>Editar</Button>
                <Button appearance="secondary" size="small" onClick={() => void restablecerPassword(usuario)}>Contraseña temporal</Button>
                {usuario.activo && <Button appearance="secondary" size="small" onClick={() => void desactivar(usuario)}>Desactivar</Button>}
              </div>
            </div>
          </article>
        ))}
      </div>
    </div>
  );
}
