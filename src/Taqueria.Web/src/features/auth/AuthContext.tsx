import { createContext, useContext, useMemo, useState, type ReactNode } from 'react';
import type { AuthResponse, UsuarioSesion } from '../../models/api';
import { apiClient } from '../../services/apiClient';
import { authStorage } from '../../services/authStorage';

interface AuthContextValue {
  usuario: UsuarioSesion | null;
  autenticado: boolean;
  login(nombreUsuario: string, password: string): Promise<UsuarioSesion>;
  cambiarPassword(passwordActual: string, nuevoPassword: string, confirmarPassword: string): Promise<void>;
  logout(): void;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [session, setSession] = useState<AuthResponse | null>(() => authStorage.get());

  const value = useMemo<AuthContextValue>(() => ({
    usuario: session?.usuario ?? null,
    autenticado: session !== null,
    async login(nombreUsuario: string, password: string) {
      const response = await apiClient.post<AuthResponse>('/api/auth/login', { nombreUsuario, password });
      authStorage.set(response);
      setSession(response);
      return response.usuario;
    },
    async cambiarPassword(passwordActual: string, nuevoPassword: string, confirmarPassword: string) {
      const response = await apiClient.post<AuthResponse>('/api/auth/cambiar-password', {
        passwordActual,
        nuevoPassword,
        confirmarPassword
      });
      authStorage.set(response);
      setSession(response);
    },
    logout() {
      authStorage.clear();
      setSession(null);
    }
  }), [session]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth requiere AuthProvider.');
  }
  return context;
}
