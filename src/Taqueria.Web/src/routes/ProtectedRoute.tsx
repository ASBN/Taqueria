import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '../features/auth/AuthContext';

export function ProtectedRoute({ permitirCambioPassword = false }: { permitirCambioPassword?: boolean }) {
  const { autenticado, usuario } = useAuth();

  if (!autenticado) {
    return <Navigate to="/login" replace />;
  }

  if (usuario?.debeCambiarPassword && !permitirCambioPassword) {
    return <Navigate to="/cambiar-password" replace />;
  }

  if (!usuario?.debeCambiarPassword && permitirCambioPassword) {
    return <Navigate to="/" replace />;
  }

  return <Outlet />;
}
