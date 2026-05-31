import { useEffect, useState, type FormEvent } from 'react';
import { Button, MessageBar, MessageBarBody, Spinner } from '@fluentui/react-components';
import type { CategoriaProducto, PrecioProducto, Producto } from '../models/api';
import { apiClient } from '../services/apiClient';
import { currency, localInputToUtcIso, toDateTimeLocal } from '../utils/format';

type Tab = 'productos' | 'categorias';

interface CategoriaForm {
  id?: number;
  nombre: string;
  ordenVisual: number;
  activa: boolean;
}

interface ProductoForm {
  id?: number;
  categoriaProductoId: number;
  nombre: string;
  ordenVisual: number;
  activo: boolean;
}

interface PrecioForm {
  id?: number;
  precio: number;
  vigenteDesdeUtc: string;
}

export function CatalogosPage() {
  const [tab, setTab] = useState<Tab>('productos');
  const [categorias, setCategorias] = useState<CategoriaProducto[]>([]);
  const [productos, setProductos] = useState<Producto[]>([]);
  const [precios, setPrecios] = useState<PrecioProducto[]>([]);
  const [productoSeleccionado, setProductoSeleccionado] = useState<Producto | null>(null);
  const [categoriaForm, setCategoriaForm] = useState<CategoriaForm | null>(null);
  const [productoForm, setProductoForm] = useState<ProductoForm | null>(null);
  const [precioForm, setPrecioForm] = useState<PrecioForm | null>(null);
  const [cargando, setCargando] = useState(true);
  const [mensaje, setMensaje] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function cargarCatalogos() {
    setCargando(true);
    try {
      const [categoriasResultado, productosResultado] = await Promise.all([
        apiClient.get<CategoriaProducto[]>('/api/categorias-producto'),
        apiClient.get<Producto[]>('/api/productos')
      ]);
      setCategorias(categoriasResultado);
      setProductos(productosResultado);
      if (productoSeleccionado) {
        const actualizado = productosResultado.find((producto) => producto.id === productoSeleccionado.id) ?? null;
        setProductoSeleccionado(actualizado);
      }
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible cargar catálogos.');
    } finally {
      setCargando(false);
    }
  }

  useEffect(() => {
    void cargarCatalogos();
  }, []);

  async function seleccionarProducto(producto: Producto) {
    setProductoSeleccionado(producto);
    setPrecioForm(null);
    setPrecios(await apiClient.get<PrecioProducto[]>(`/api/productos/${producto.id}/precios`));
  }

  function nuevaCategoria() {
    setCategoriaForm({ nombre: '', ordenVisual: categorias.length + 1, activa: true });
  }

  async function guardarCategoria(event: FormEvent) {
    event.preventDefault();
    if (!categoriaForm) {
      return;
    }
    try {
      if (categoriaForm.id) {
        await apiClient.put(`/api/categorias-producto/${categoriaForm.id}`, categoriaForm);
      } else {
        await apiClient.post('/api/categorias-producto', categoriaForm);
      }
      setMensaje('Categoría guardada.');
      setCategoriaForm(null);
      await cargarCatalogos();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible guardar la categoría.');
    }
  }

  async function eliminarCategoria(categoria: CategoriaProducto) {
    if (!window.confirm(`¿Retirar la categoría ${categoria.nombre}?`)) {
      return;
    }
    await apiClient.delete(`/api/categorias-producto/${categoria.id}`);
    setMensaje('Categoría retirada.');
    await cargarCatalogos();
  }

  function nuevoProducto() {
    const primeraCategoriaActiva = categorias.find((categoria) => categoria.activa);
    if (!primeraCategoriaActiva) {
      setError('Cree una categoría activa antes de registrar productos.');
      return;
    }
    setProductoForm({
      categoriaProductoId: primeraCategoriaActiva.id,
      nombre: '',
      ordenVisual: productos.length + 1,
      activo: true
    });
  }

  async function guardarProducto(event: FormEvent) {
    event.preventDefault();
    if (!productoForm) {
      return;
    }
    try {
      if (productoForm.id) {
        await apiClient.put(`/api/productos/${productoForm.id}`, productoForm);
      } else {
        await apiClient.post('/api/productos', productoForm);
      }
      setMensaje('Producto guardado.');
      setProductoForm(null);
      await cargarCatalogos();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible guardar el producto.');
    }
  }

  async function eliminarProducto(producto: Producto) {
    if (!window.confirm(`¿Retirar el producto ${producto.nombre}?`)) {
      return;
    }
    await apiClient.delete(`/api/productos/${producto.id}`);
    setMensaje('Producto retirado.');
    await cargarCatalogos();
  }

  function nuevoPrecio() {
    setPrecioForm({
      precio: productoSeleccionado?.precioVigente ?? 1,
      vigenteDesdeUtc: toDateTimeLocal(new Date().toISOString())
    });
  }

  async function guardarPrecio(event: FormEvent) {
    event.preventDefault();
    if (!productoSeleccionado || !precioForm) {
      return;
    }

    const dto = {
      precio: precioForm.precio,
      vigenteDesdeUtc: localInputToUtcIso(precioForm.vigenteDesdeUtc)
    };

    try {
      if (precioForm.id) {
        await apiClient.put(`/api/precios-producto/${precioForm.id}`, dto);
      } else {
        await apiClient.post(`/api/productos/${productoSeleccionado.id}/precios`, dto);
      }

      setMensaje('Precio guardado. El historial anterior permanece intacto.');
      setPrecioForm(null);
      await seleccionarProducto(productoSeleccionado);
      await cargarCatalogos();
    } catch (exception) {
      setError(exception instanceof Error ? exception.message : 'No fue posible guardar el precio.');
    }
  }

  async function eliminarPrecio(precio: PrecioProducto) {
    if (!window.confirm('¿Eliminar este precio futuro programado?')) {
      return;
    }

    await apiClient.delete(`/api/precios-producto/${precio.id}`);
    if (productoSeleccionado) {
      await seleccionarProducto(productoSeleccionado);
      await cargarCatalogos();
    }
  }

  const ahora = Date.now();

  return (
    <div className="mx-auto page-width">
      <div className="mb-3">
        <h1 className="h4 mb-1">Catálogos</h1>
        <p className="text-secondary small mb-0">Productos y precios históricos</p>
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

      <ul className="nav nav-pills nav-fill bg-white rounded-3 p-1 shadow-sm mb-3">
        <li className="nav-item">
          <button className={`nav-link ${tab === 'productos' ? 'active' : ''}`} onClick={() => setTab('productos')}>
            Productos
          </button>
        </li>
        <li className="nav-item">
          <button className={`nav-link ${tab === 'categorias' ? 'active' : ''}`} onClick={() => setTab('categorias')}>
            Categorías
          </button>
        </li>
      </ul>

      {cargando ? (
        <div className="text-center py-5"><Spinner label="Cargando catálogos..." /></div>
      ) : tab === 'categorias' ? (
        <>
          <div className="d-flex justify-content-end mb-3">
            <Button appearance="primary" onClick={nuevaCategoria}>Nueva categoría</Button>
          </div>
          {categoriaForm && (
            <form className="card border-0 shadow-sm mb-3" onSubmit={guardarCategoria}>
              <div className="card-body">
                <h2 className="h6">{categoriaForm.id ? 'Editar categoría' : 'Nueva categoría'}</h2>
                <input className="form-control form-control-lg mb-2" placeholder="Nombre" required value={categoriaForm.nombre}
                  onChange={(event) => setCategoriaForm({ ...categoriaForm, nombre: event.target.value })} />
                <input className="form-control mb-3" type="number" min="0" value={categoriaForm.ordenVisual}
                  onChange={(event) => setCategoriaForm({ ...categoriaForm, ordenVisual: Number(event.target.value) })} />
                <div className="d-flex gap-2">
                  <Button appearance="primary" type="submit">Guardar</Button>
                  <Button onClick={() => setCategoriaForm(null)}>Cancelar</Button>
                </div>
              </div>
            </form>
          )}
          <div className="d-grid gap-2">
            {categorias.map((categoria) => (
              <article className={`card border-0 shadow-sm ${!categoria.activa ? 'inactive-card' : ''}`} key={categoria.id}>
                <div className="card-body d-flex align-items-center gap-3">
                  <span className="sort-number">{categoria.ordenVisual}</span>
                  <div className="flex-grow-1">
                    <strong>{categoria.nombre}</strong>
                    {!categoria.activa && <span className="badge text-bg-secondary ms-2">Inactiva</span>}
                  </div>
                  <button className="btn btn-sm btn-outline-secondary" onClick={() => setCategoriaForm({ ...categoria })}><i className="bi bi-pencil" /></button>
                  <button className="btn btn-sm btn-outline-danger" onClick={() => void eliminarCategoria(categoria)}><i className="bi bi-trash" /></button>
                </div>
              </article>
            ))}
          </div>
        </>
      ) : (
        <>
          <div className="d-flex justify-content-end mb-3">
            <Button appearance="primary" onClick={nuevoProducto}>Nuevo producto</Button>
          </div>
          {productoForm && (
            <form className="card border-0 shadow-sm mb-3" onSubmit={guardarProducto}>
              <div className="card-body d-grid gap-2">
                <h2 className="h6">{productoForm.id ? 'Editar producto' : 'Nuevo producto'}</h2>
                <select className="form-select form-select-lg" required value={productoForm.categoriaProductoId}
                  onChange={(event) => setProductoForm({ ...productoForm, categoriaProductoId: Number(event.target.value) })}>
                  {categorias.filter((categoria) => categoria.activa || categoria.id === productoForm.categoriaProductoId).map((categoria) => (
                    <option key={categoria.id} value={categoria.id}>{categoria.nombre}</option>
                  ))}
                </select>
                <input className="form-control form-control-lg" placeholder="Nombre del producto" required value={productoForm.nombre}
                  onChange={(event) => setProductoForm({ ...productoForm, nombre: event.target.value })} />
                <input className="form-control" type="number" min="0" value={productoForm.ordenVisual}
                  onChange={(event) => setProductoForm({ ...productoForm, ordenVisual: Number(event.target.value) })} />
                <div className="d-flex gap-2 mt-2">
                  <Button appearance="primary" type="submit">Guardar</Button>
                  <Button onClick={() => setProductoForm(null)}>Cancelar</Button>
                </div>
              </div>
            </form>
          )}

          <div className="d-grid gap-2 mb-4">
            {productos.map((producto) => (
              <article className={`card border-0 shadow-sm ${!producto.activo ? 'inactive-card' : ''}`} key={producto.id}>
                <div className="card-body">
                  <div className="d-flex align-items-start gap-2">
                    <button className="product-select text-start flex-grow-1" onClick={() => void seleccionarProducto(producto)}>
                      <div className="small text-secondary">{producto.categoriaNombre}</div>
                      <div className="fw-semibold">{producto.nombre}</div>
                      <div className="price-label">{currency(producto.precioVigente)}</div>
                    </button>
                    <button className="btn btn-sm btn-outline-secondary" onClick={() => setProductoForm({ ...producto })}><i className="bi bi-pencil" /></button>
                    <button className="btn btn-sm btn-outline-danger" onClick={() => void eliminarProducto(producto)}><i className="bi bi-trash" /></button>
                  </div>
                </div>
              </article>
            ))}
          </div>

          {productoSeleccionado && (
            <section className="card border-0 shadow-sm mb-3">
              <div className="card-body">
                <div className="d-flex justify-content-between align-items-center mb-3">
                  <div>
                    <h2 className="h6 mb-1">Precios · {productoSeleccionado.nombre}</h2>
                    <span className="small text-secondary">Nunca se sobreescribe un precio vigente</span>
                  </div>
                  <Button appearance="primary" onClick={nuevoPrecio}>Agregar</Button>
                </div>

                {precioForm && (
                  <form className="border rounded-3 p-3 mb-3" onSubmit={guardarPrecio}>
                    <div className="row g-2">
                      <div className="col-5">
                        <label className="form-label small">Precio</label>
                        <input className="form-control" type="number" min="0.01" step="0.01" required value={precioForm.precio}
                          onChange={(event) => setPrecioForm({ ...precioForm, precio: Number(event.target.value) })} />
                      </div>
                      <div className="col-7">
                        <label className="form-label small">Vigente desde</label>
                        <input className="form-control" type="datetime-local" required value={precioForm.vigenteDesdeUtc}
                          onChange={(event) => setPrecioForm({ ...precioForm, vigenteDesdeUtc: event.target.value })} />
                      </div>
                    </div>
                    <div className="d-flex gap-2 mt-3">
                      <Button appearance="primary" type="submit">Guardar</Button>
                      <Button onClick={() => setPrecioForm(null)}>Cancelar</Button>
                    </div>
                  </form>
                )}

                <div className="d-grid gap-2">
                  {precios.map((precio) => {
                    const futuro = new Date(precio.vigenteDesdeUtc).getTime() > ahora;
                    return (
                      <div className={`price-history-item ${precio.esVigente ? 'current' : ''}`} key={precio.id}>
                        <div>
                          <strong>{currency(precio.precio)}</strong>
                          <div className="small text-secondary">
                            Desde {new Date(precio.vigenteDesdeUtc).toLocaleString('es-MX')}
                          </div>
                        </div>
                        <div className="d-flex align-items-center gap-2">
                          {precio.esVigente && <span className="badge text-bg-success">Vigente</span>}
                          {futuro && (
                            <>
                              <button className="btn btn-sm btn-outline-secondary" onClick={() => setPrecioForm({
                                id: precio.id,
                                precio: precio.precio,
                                vigenteDesdeUtc: toDateTimeLocal(precio.vigenteDesdeUtc)
                              })}><i className="bi bi-pencil" /></button>
                              <button className="btn btn-sm btn-outline-danger" onClick={() => void eliminarPrecio(precio)}><i className="bi bi-trash" /></button>
                            </>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </div>
            </section>
          )}
        </>
      )}
    </div>
  );
}
