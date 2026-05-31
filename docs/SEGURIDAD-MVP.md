# Seguridad del MVP

## Incluido hasta ZIP-007 RC2

- Hash de contraseña PBKDF2 con sal aleatoria.
- JWT firmado y validado por issuer, audience, firma y expiración.
- Autorización por roles `Administrador`, `Mesero`, `Cocina` y `Caja`.
- Contraseña temporal obligatoria para usuarios seed, nuevos o restablecidos.
- Validación del estado actual del usuario en cada petición autenticada: un usuario desactivado o pendiente de cambio de contraseña no continúa operando con un token anterior.
- Protección para conservar al menos un administrador activo.
- Baja lógica de usuarios para no perder trazabilidad de ventas y operación.
- Cancelaciones con motivo, fecha y usuario responsable.
- Merma de productos preparados no entregados al cancelar, con responsable e importe histórico de referencia.
- Pago mixto limitado a métodos permitidos y validado contra el total histórico de la cuenta.

## Alcances deliberados del MVP

La validación de vigencia del usuario realiza una lectura ligera a base de datos por petición autenticada. Para el volumen de una taquería pequeña-mediana prioriza seguridad y claridad sobre optimización prematura.

No se implementa todavía:

- Recuperación autónoma de contraseña.
- Bitácora general de cambios administrativos.
- Devolución o anulación de cobros.
- Revocación distribuida o rotación avanzada de tokens.

## Antes de producción pública

- Extraer llave JWT fuera de `appsettings.json` y administrarla como secreto.
- Forzar HTTPS extremo a extremo.
- Establecer política real de contraseñas y recuperación segura.
- Incorporar respaldo y restauración probados de la base de datos.
- Evaluar identidad externa o patrón BFF si el sistema se expone fuera de la red operativa.


## Persistencia SQLite — RC1

La base local se guarda fuera del directorio publicado de la API. En almacenamiento local se activa WAL y timeout para reducir errores por bloqueo; en Azure HOME WAL permanece deshabilitado y SQLite debe limitarse a una sola instancia ligera antes de migrar a SQL Server/Azure SQL.
