# Feature Caja

RC2 conserva el circuito independiente de cobro:

1. El mesero confirma entregas parciales y envía una cuenta terminada a Caja.
2. Caja sólo recibe comandas en estado `PendienteCobro`.
3. Para cobro simple, Caja elige el método en un toque y backend registra el total histórico.
4. Para cobro mixto, Caja distribuye el total entre efectivo, tarjeta y transferencia; el botón se habilita sólo cuando la suma coincide exactamente.
5. Al registrar los pagos, la comanda pasa a `Cobrada` y la mesa vuelve a estar libre.
