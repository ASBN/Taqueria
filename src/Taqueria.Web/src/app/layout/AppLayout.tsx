import { Button } from '@fluentui/react-components';
import { Home24Regular, SignOut24Regular } from '@fluentui/react-icons';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../../features/auth/AuthContext';

export function AppLayout() {
  const { usuario, logout } = useAuth();
  const navigate = useNavigate();

  function cerrarSesion() {
    logout();
    navigate('/login');
  }

  return (
    <div className="app-shell">
      <header className="navbar navbar-dark brand-navbar sticky-top shadow-sm">
        <div className="container-fluid">
          <span className="navbar-brand mb-0 fw-bold">
            <i className="bi bi-fire me-2" />
            Taquería
          </span>
          <div className="d-flex align-items-center gap-2">
            <span className="small text-white d-none d-sm-inline">{usuario?.nombre}</span>
            <Button
              appearance="subtle"
              icon={<SignOut24Regular />}
              aria-label="Cerrar sesión"
              onClick={cerrarSesion}
            />
          </div>
        </div>
      </header>

      <main className="container-fluid px-3 pt-3 pb-navigation-space">
        <Outlet />
      </main>

      <nav className="mobile-nav fixed-bottom bg-white border-top shadow-lg">
        <NavLink to="/" end className="mobile-nav-link">
          <Home24Regular />
          <span>Inicio</span>
        </NavLink>
        {(usuario?.rol === 'Administrador' || usuario?.rol === 'Mesero') && (
          <NavLink to="/comandas" className="mobile-nav-link">
            <i className="bi bi-receipt fs-4" />
            <span>Comandas</span>
          </NavLink>
        )}
        {(usuario?.rol === 'Administrador' || usuario?.rol === 'Cocina') && (
          <NavLink to="/cocina" className="mobile-nav-link">
            <i className="bi bi-fire fs-4" />
            <span>Cocina</span>
          </NavLink>
        )}
        {(usuario?.rol === 'Administrador' || usuario?.rol === 'Caja') && (
          <NavLink to="/caja" className="mobile-nav-link">
            <i className="bi bi-cash-coin fs-4" />
            <span>Caja</span>
          </NavLink>
        )}
        {usuario?.rol === 'Caja' && (
          <NavLink to="/reportes" className="mobile-nav-link">
            <i className="bi bi-file-earmark-spreadsheet fs-4" />
            <span>Corte</span>
          </NavLink>
        )}
        {usuario?.rol === 'Administrador' && (
          <NavLink to="/mesas" className="mobile-nav-link">
            <i className="bi bi-grid-3x3-gap fs-4" />
            <span>Mesas</span>
          </NavLink>
        )}
        {usuario?.rol === 'Administrador' && <NavLink to="/catalogos" className="mobile-nav-link">
          <i className="bi bi-tags fs-4" />
          <span>Catálogo</span>
        </NavLink>}
        {usuario?.rol === 'Administrador' && <NavLink to="/usuarios" className="mobile-nav-link">
          <i className="bi bi-people fs-4" />
          <span>Usuarios</span>
        </NavLink>}
      </nav>
    </div>
  );
}
