# Migraciones — RC1

Reinicia la base de desarrollo con una única migración inicial SQLite-compatible:

- `202605310001_InitialCreateRc1` crea el esquema completo acumulado hasta RC1.
- Incluye categoría histórica en `ComandaDetalle`.
- Incluye `UsuarioCancelacionId` y su llave foránea dentro de la creación original de `Comandas`, evitando el `AddForeignKeyOperation` incremental que SQLite no pudo aplicar.
