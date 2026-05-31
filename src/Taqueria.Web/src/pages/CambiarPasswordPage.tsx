import { useState, type FormEvent } from 'react';
import { Button, Field, Input, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../features/auth/AuthContext';

export function CambiarPasswordPage() {
  const [passwordActual, setPasswordActual] = useState('');
  const [nuevoPassword, setNuevoPassword] = useState('');
  const [confirmarPassword, setConfirmarPassword] = useState('');
  const [procesando, setProcesando] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const { usuario, cambiarPassword, logout } = useAuth();
  const navigate = useNavigate();

  async function guardar(event: FormEvent) {
    event.preventDefault();
    setProcesando(true);
    setError(null);
    try {
      await cambiarPassword(passwordActual, nuevoPassword, confirmarPassword);
      navigate('/');
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible cambiar la contraseña.');
    } finally {
      setProcesando(false);
    }
  }

  function salir() {
    logout();
    navigate('/login');
  }

  return (
    <div className="login-background min-vh-100 d-flex align-items-center justify-content-center px-3">
      <section className="card login-card shadow-lg border-0">
        <div className="card-body p-4">
          <h1 className="h4 mb-2">Cambie su contraseña</h1>
          <p className="text-secondary small mb-4">
            Hola, {usuario?.nombre}. Antes de operar debe reemplazar la contraseña temporal.
          </p>

          {error && <MessageBar intent="error" className="mb-3"><MessageBarBody>{error}</MessageBarBody></MessageBar>}

          <form onSubmit={guardar} className="d-grid gap-3">
            <Field label="Contraseña temporal" required>
              <Input type="password" size="large" value={passwordActual} onChange={(_, data) => setPasswordActual(data.value)} />
            </Field>
            <Field label="Nueva contraseña" hint="Mínimo 8 caracteres, con letras y números." required>
              <Input type="password" size="large" value={nuevoPassword} onChange={(_, data) => setNuevoPassword(data.value)} />
            </Field>
            <Field label="Confirmar nueva contraseña" required>
              <Input type="password" size="large" value={confirmarPassword} onChange={(_, data) => setConfirmarPassword(data.value)} />
            </Field>
            <Button type="submit" appearance="primary" size="large" disabled={procesando}>
              {procesando ? <Spinner size="tiny" /> : 'Guardar y continuar'}
            </Button>
            <Button type="button" appearance="secondary" size="large" onClick={salir}>Cerrar sesión</Button>
          </form>
        </div>
      </section>
    </div>
  );
}
