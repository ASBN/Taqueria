# DTOs de Cobros

ZIP-007 mantiene Caja como módulo independiente y activa pago mixto.

- Pago sencillo: el cliente puede seguir enviando `MetodoPago`; el backend registra automáticamente el total completo de la cuenta.
- Pago mixto: el cliente envía `Pagos` con una distribución entre `Efectivo`, `Tarjeta` y `Transferencia`.
- La suma de importes debe coincidir exactamente con el total histórico de la comanda.
- El DTO de salida devuelve la colección de pagos registrados.
