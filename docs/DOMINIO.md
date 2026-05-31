# Dominio operativo — Taquería

## Agregado Comanda

`Comanda` concentra el ciclo de servicio de una mesa y contiene:

```text
Comanda
 ├── Comensal
 │    └── ComandaDetalle
 │         ├── EntregaParcial
 │         └── MermaComandaDetalle
 └── PagoComanda
```

Una mesa mantiene una única comanda activa hasta que queda `Cobrada` o `Cancelada`.

## Fronteras operativas

| Rol | Responsabilidad |
|---|---|
| Mesero | Abrir mesa, capturar pedido, confirmar entrega a cliente, enviar cuenta a Caja y cancelar únicamente antes de cocina |
| Cocina | Preparar partidas y liberar lotes parciales |
| Caja | Cobrar cuentas enviadas, registrar pagos simples o mixtos, liberar mesa y generar corte diario |
| Administrador | Mantener catálogos/usuarios, consultar reportes y cancelar órdenes; si ya se prepararon productos no entregados, registra merma automática |

Caja nunca modifica pedidos ni confirma entregas. El mesero nunca captura importes pagados.

## Usuarios y acceso

`Usuario` conserva rol, estado activo y obligación de cambiar contraseña. Los accesos se desactivan sin eliminar al usuario porque sus identificadores forman parte del histórico de operación.

```text
Contraseña temporal → cambio obligatorio → operación habilitada
Usuario desactivado → token existente rechazado en la siguiente petición
```

## Snapshot histórico de una partida

Al agregar una partida el backend copia desde el catálogo:

```text
CategoriaProductoIdHistorico
NombreCategoriaHistorico
NombreProductoHistorico
PrecioUnitarioHistorico
Subtotal
```

Un cambio posterior de nombre, precio o categoría no altera el registro vendido.

## Cocina y entrega a mesa

`CantidadPreparada` aumenta cuando Cocina libera un `EntregaParcial`. `CantidadEntregada` aumenta únicamente cuando Mesero confirma que ese lote llegó a mesa.

```text
Pedido: 50 tacos pastor
Cocina libera: 20 → Mesero entrega: 20
Cocina libera: 15 → Mesero entrega: 15
Cocina libera: 15 → Mesero entrega: 15
```

## Cobro y corte diario

Caja puede registrar un método único por el total completo o distribuir la cuenta entre efectivo, tarjeta y transferencia. La invariante es `Suma(PagoComanda.Importe) == Comanda.CalcularTotal()`. El reporte diario consulta exclusivamente comandas `Cobrada` y agrupa desde la evidencia histórica de cada partida y pago.

## Cancelación controlada

Toda cancelación almacena motivo, fecha UTC y usuario responsable. La regla evita esconder producción o cobros que requieren un tratamiento contable/operativo adicional:

```text
Abierta + Mesero/Admin                     → Cancelada sin merma
EnPreparacion sin preparación + Admin       → Cancelada sin merma
CantidadPreparada > 0 y Entregada == 0      → Cancelada + MermaComandaDetalle por partida
CantidadEntregada > 0 o Cobrada             → Rechazada hasta implementar devolución
```

## Estados activos hasta ZIP-007 RC2

```text
Abierta
  ├── Cancelada
  └── EnPreparacion
       ├── Cancelada (Admin; genera merma si hubo preparación no entregada)
       ├── ParcialmenteLista
       │    └── Lista
       └── Lista
             └── Entregada
                   └── PendienteCobro
                         └── Cobrada
```

## Invariantes

```text
0 <= CantidadEntregada <= CantidadPreparada <= Cantidad
Suma(PagoComanda.Importe) == Comanda.CalcularTotal()
Mesa ocupada mientras Estado != Cobrada && Estado != Cancelada
Reporte diario incluye sólo comandas Estado == Cobrada
Reporte por categoría usa NombreCategoriaHistorico, no catálogo actual
Comanda cancelada con motivo conserva UsuarioCancelacionId
Cancelación con producción preparada no entregada crea MermaComandaDetalle; una entrega confirmada o venta cobrada requiere devolución formal
```
