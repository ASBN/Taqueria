import { Link } from 'react-router-dom';
import { useAuth } from '../features/auth/AuthContext';

export function DashboardPage() {
  const { usuario } = useAuth();
  const puedeCapturar = usuario?.rol === 'Administrador' || usuario?.rol === 'Mesero';
  const puedeCocinar = usuario?.rol === 'Administrador' || usuario?.rol === 'Cocina';
  const puedeCobrar = usuario?.rol === 'Administrador' || usuario?.rol === 'Caja';
  const puedeReportar = puedeCobrar;

  const mensaje = puedeCobrar && !puedeCapturar && !puedeCocinar
    ? 'Ya puede consultar cuentas pendientes, cobrar y liberar mesas desde caja.'
    : puedeCocinar && !puedeCapturar
      ? 'Ya puede consultar pedidos y liberar cantidades preparadas desde cocina.'
      : 'Ya puede atender mesas, entregar partidas listas y enviar cuentas a caja.';

  return (
    <div className="mx-auto page-width">
      <section className="welcome-card card border-0 shadow-sm mb-3">
        <div className="card-body p-4">
          <p className="text-uppercase small fw-semibold mb-2 text-brand">Operación diaria RC2</p>
          <h1 className="h4 mb-2">Hola, {usuario?.nombre}</h1>
          <p className="text-secondary mb-0">{mensaje}</p>
        </div>
      </section>

      <div className="row g-3 mb-4">
        {puedeCapturar && (
          <div className="col-6">
            <Link to="/comandas" className="text-decoration-none">
              <article className="action-card card h-100 border-0 shadow-sm">
                <div className="card-body">
                  <i className="bi bi-receipt action-icon" />
                  <h2 className="h6 mt-3 mb-1">Comandas</h2>
                  <span className="small text-secondary">Entregar y enviar cuenta</span>
                </div>
              </article>
            </Link>
          </div>
        )}
        {puedeCocinar && (
          <div className="col-6">
            <Link to="/cocina" className="text-decoration-none">
              <article className="action-card card h-100 border-0 shadow-sm">
                <div className="card-body">
                  <i className="bi bi-fire action-icon" />
                  <h2 className="h6 mt-3 mb-1">Cocina</h2>
                  <span className="small text-secondary">Liberar partidas</span>
                </div>
              </article>
            </Link>
          </div>
        )}
        {puedeCobrar && (
          <div className="col-6">
            <Link to="/caja" className="text-decoration-none">
              <article className="action-card card h-100 border-0 shadow-sm">
                <div className="card-body">
                  <i className="bi bi-cash-coin action-icon" />
                  <h2 className="h6 mt-3 mb-1">Caja</h2>
                  <span className="small text-secondary">Cobrar y liberar mesa</span>
                </div>
              </article>
            </Link>
          </div>
        )}
        {puedeReportar && (
          <div className="col-6">
            <Link to="/reportes" className="text-decoration-none">
              <article className="action-card card h-100 border-0 shadow-sm">
                <div className="card-body">
                  <i className="bi bi-file-earmark-spreadsheet action-icon" />
                  <h2 className="h6 mt-3 mb-1">Reportes</h2>
                  <span className="small text-secondary">Ventas y mermas Excel</span>
                </div>
              </article>
            </Link>
          </div>
        )}
        {usuario?.rol === 'Administrador' && (
          <>
            <div className="col-6">
              <Link to="/mesas" className="text-decoration-none">
                <article className="action-card card h-100 border-0 shadow-sm">
                  <div className="card-body">
                    <i className="bi bi-grid-3x3-gap action-icon" />
                    <h2 className="h6 mt-3 mb-1">Mesas</h2>
                    <span className="small text-secondary">Configurar salón</span>
                  </div>
                </article>
              </Link>
            </div>
            <div className="col-6">
              <Link to="/catalogos" className="text-decoration-none">
                <article className="action-card card h-100 border-0 shadow-sm">
                  <div className="card-body">
                    <i className="bi bi-tags action-icon" />
                    <h2 className="h6 mt-3 mb-1">Productos</h2>
                    <span className="small text-secondary">Precios y catálogo</span>
                  </div>
                </article>
              </Link>
            </div>
            <div className="col-6">
              <Link to="/usuarios" className="text-decoration-none">
                <article className="action-card card h-100 border-0 shadow-sm">
                  <div className="card-body">
                    <i className="bi bi-people action-icon" />
                    <h2 className="h6 mt-3 mb-1">Usuarios</h2>
                    <span className="small text-secondary">Accesos y roles</span>
                  </div>
                </article>
              </Link>
            </div>
          </>
        )}
      </div>
    </div>
  );
}
