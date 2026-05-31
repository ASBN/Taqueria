export interface UsuarioSesion {
  id: number;
  nombre: string;
  nombreUsuario: string;
  rol: string;
  activo: boolean;
  debeCambiarPassword: boolean;
}

export interface AuthResponse {
  token: string;
  expiraUtc: string;
  usuario: UsuarioSesion;
}

export interface Usuario {
  id: number;
  nombre: string;
  nombreUsuario: string;
  rol: string;
  activo: boolean;
  debeCambiarPassword: boolean;
  fechaCreacionUtc: string;
}

export interface Mesa {
  id: number;
  numero: number;
  capacidad: number;
  activa: boolean;
}

export interface CategoriaProducto {
  id: number;
  nombre: string;
  ordenVisual: number;
  activa: boolean;
}

export interface Producto {
  id: number;
  categoriaProductoId: number;
  categoriaNombre: string;
  nombre: string;
  ordenVisual: number;
  activo: boolean;
  precioVigente: number | null;
}

export interface PrecioProducto {
  id: number;
  productoId: number;
  precio: number;
  vigenteDesdeUtc: string;
  fechaCreacionUtc: string;
  esVigente: boolean;
}

export interface EntregaParcialMesero {
  id: number;
  cantidad: number;
  fechaHoraListaUtc: string;
  fechaHoraEntregaUtc: string | null;
  estado: string;
}

export interface MermaComandaDetalle {
  id: number;
  cantidad: number;
  importeHistorico: number;
  motivo: string;
  fechaHoraRegistroUtc: string;
  registradaPor: string;
}

export interface ComandaDetalle {
  id: number;
  productoId: number;
  categoriaProductoIdHistorico: number;
  nombreCategoriaHistorico: string;
  nombreProductoHistorico: string;
  precioUnitarioHistorico: number;
  cantidad: number;
  cantidadPreparada: number;
  cantidadEntregada: number;
  subtotal: number;
  entregasParciales: EntregaParcialMesero[];
  mermas: MermaComandaDetalle[];
}

export interface Comensal {
  id: number;
  numero: number;
  activo: boolean;
  subtotal: number;
  detalles: ComandaDetalle[];
}

export interface Comanda {
  id: number;
  folio: string;
  mesaId: number;
  mesaNumero: number;
  usuarioMeseroId: number;
  mesero: string;
  estado: string;
  fechaHoraAperturaUtc: string;
  fechaHoraEnvioCocinaUtc: string | null;
  fechaHoraEntregaUtc: string | null;
  fechaHoraCancelacionUtc: string | null;
  motivoCancelacion: string | null;
  canceladaPor: string | null;
  total: number;
  comensales: Comensal[];
}

export interface EntregaParcialCocina {
  id: number;
  cantidad: number;
  fechaHoraListaUtc: string;
  estado: string;
  usuarioCocina: string;
}

export interface PartidaCocina {
  id: number;
  comensalId: number;
  numeroComensal: number;
  producto: string;
  cantidad: number;
  cantidadPreparada: number;
  cantidadPendiente: number;
  entregasParciales: EntregaParcialCocina[];
}

export interface ComensalCocina {
  id: number;
  numero: number;
  partidas: PartidaCocina[];
}

export interface ComandaCocina {
  id: number;
  folio: string;
  mesaId: number;
  mesaNumero: number;
  mesero: string;
  estado: string;
  fechaHoraEnvioCocinaUtc: string | null;
  totalProductos: number;
  totalPreparados: number;
  totalPendientes: number;
  comensales: ComensalCocina[];
}

export interface CuentaPendienteCobro {
  comandaId: number;
  folio: string;
  mesaId: number;
  mesaNumero: number;
  mesero: string;
  estado: string;
  fechaHoraAperturaUtc: string;
  fechaHoraEntregaUtc: string | null;
  fechaHoraCancelacionUtc: string | null;
  motivoCancelacion: string | null;
  canceladaPor: string | null;
  total: number;
  totalPartidas: number;
  totalProductos: number;
}

export interface PagoComanda {
  id: number;
  metodoPago: string;
  importe: number;
  fechaHoraPagoUtc: string;
  usuarioCobro: string;
}

export interface CobroRegistrado {
  comandaId: number;
  mesaNumero: number;
  estado: string;
  totalCobrado: number;
  fechaHoraCobroUtc: string | null;
  pagos: PagoComanda[];
}

export interface ResumenVentasDia {
  numeroVentas: number;
  productosVendidos: number;
  total: number;
  ticketPromedio: number;
}

export interface VentaDia {
  fechaHoraCobroUtc: string;
  mesa: number;
  comanda: string;
  metodoPago: string;
  total: number;
  mesero: string;
  horaAperturaUtc: string;
  horaCobroUtc: string;
}

export interface VentaProducto {
  productoId: number;
  producto: string;
  categoria: string;
  cantidad: number;
  importe: number;
  precioPromedio: number;
}

export interface VentaCategoria {
  categoriaProductoId: number;
  categoria: string;
  cantidad: number;
  importe: number;
}

export interface VentaMesero {
  usuarioMeseroId: number;
  mesero: string;
  numeroVentas: number;
  productosVendidos: number;
  importe: number;
}

export interface VentaMetodoPago {
  metodoPago: string;
  numeroVentas: number;
  importe: number;
}

export interface ReporteVentasDiario {
  fecha: string;
  desfaseHorarioMinutos: number;
  desdeUtc: string;
  hastaUtcExclusiva: string;
  resumen: ResumenVentasDia;
  ventas: VentaDia[];
  porProducto: VentaProducto[];
  porCategoria: VentaCategoria[];
  porMesero: VentaMesero[];
  porMetodoPago: VentaMetodoPago[];
}


export interface ResumenMermasDia {
  numeroCancelacionesConMerma: number;
  productosMermados: number;
  importeHistoricoReferencia: number;
}

export interface MermaDia {
  fechaHoraRegistroUtc: string;
  mesa: number;
  comanda: string;
  producto: string;
  categoria: string;
  cantidad: number;
  importeHistorico: number;
  motivo: string;
  registradaPor: string;
}

export interface ReporteMermasDiario {
  fecha: string;
  desfaseHorarioMinutos: number;
  desdeUtc: string;
  hastaUtcExclusiva: string;
  resumen: ResumenMermasDia;
  mermas: MermaDia[];
}
