import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom';
import { AuthProvider } from '../features/auth/AuthContext';
import { ProtectedRoute } from '../routes/ProtectedRoute';
import { AppLayout } from './layout/AppLayout';
import { LoginPage } from '../pages/LoginPage';
import { DashboardPage } from '../pages/DashboardPage';
import { MesasPage } from '../pages/MesasPage';
import { CatalogosPage } from '../pages/CatalogosPage';
import { ComandasPage } from '../pages/ComandasPage';
import { CocinaPage } from '../pages/CocinaPage';
import { CajaPage } from '../pages/CajaPage';
import { ReportesPage } from '../pages/ReportesPage';
import { UsuariosPage } from '../pages/UsuariosPage';
import { CambiarPasswordPage } from '../pages/CambiarPasswordPage';

export function App() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route element={<ProtectedRoute permitirCambioPassword />}>
            <Route path="/cambiar-password" element={<CambiarPasswordPage />} />
          </Route>
          <Route element={<ProtectedRoute />}>
            <Route element={<AppLayout />}>
              <Route index element={<DashboardPage />} />
              <Route path="/mesas" element={<MesasPage />} />
              <Route path="/comandas" element={<ComandasPage />} />
              <Route path="/cocina" element={<CocinaPage />} />
              <Route path="/caja" element={<CajaPage />} />
              <Route path="/reportes" element={<ReportesPage />} />
              <Route path="/catalogos" element={<CatalogosPage />} />
              <Route path="/usuarios" element={<UsuariosPage />} />
            </Route>
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
