import { useState, type FormEvent } from 'react';
import { Button, Field, Input, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../features/auth/AuthContext';

export function LoginPage() {
  const [nombreUsuario, setNombreUsuario] = useState('admin');
  const [password, setPassword] = useState('admin123');
  const [error, setError] = useState<string | null>(null);
  const [procesando, setProcesando] = useState(false);
  const { login } = useAuth();
  const navigate = useNavigate();

  async function iniciarSesion(event: FormEvent) {
    event.preventDefault();
    setProcesando(true);
    setError(null);

    try {
      const usuario = await login(nombreUsuario, password);
      navigate(usuario.debeCambiarPassword ? '/cambiar-password' : '/');
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible iniciar sesión.');
    } finally {
      setProcesando(false);
    }
  }

  return (
    <div className="login-background min-vh-100 d-flex align-items-center justify-content-center px-3">
      <section className="card login-card shadow-lg border-0">
        <div className="card-body p-4">
          <div className="text-center mb-4">
            <div className="login-icon mx-auto mb-3"><i className="bi bi-fire" /></div>
            <h1 className="h4 mb-1">Sistema de Comandas</h1>
            <p className="text-secondary mb-0">Taquería · RC2</p>
          </div>

          {error && (
            <MessageBar intent="error" className="mb-3">
              <MessageBarBody>{error}</MessageBarBody>
            </MessageBar>
          )}

          <form onSubmit={iniciarSesion} className="d-grid gap-3">
            <Field label="Usuario" required>
              <Input
                size="large"
                value={nombreUsuario}
                onChange={(_, data) => setNombreUsuario(data.value)}
                autoComplete="username"
              />
            </Field>
            <Field label="Contraseña" required>
              <Input
                size="large"
                type="password"
                value={password}
                onChange={(_, data) => setPassword(data.value)}
                autoComplete="current-password"
              />
            </Field>
            <Button type="submit" appearance="primary" size="large" disabled={procesando}>
              {procesando ? <Spinner size="tiny" /> : 'Entrar'}
            </Button>
          </form>

          <div className="alert alert-warning small mt-4 mb-0">
            Accesos iniciales: <strong>mesero / mesero123</strong>, <strong>cocina / cocina123</strong>, <strong>caja / caja123</strong> y <strong>admin / admin123</strong>.<br />
            El sistema solicitará cambiar la contraseña antes de operar.
          </div>
        </div>
      </section>
    </div>
  );
}
