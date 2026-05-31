# Taquería — Sistema de Comandas

Sistema POS de comandas para una taquería pequeña-mediana, construido como **MVP profesional evolutivo**. Su objetivo es resolver la operación diaria —mesas, cocina, caja, cancelaciones y corte— con código claro que también sirva para aprender arquitectura limpia, SOLID y desarrollo incremental con .NET + React.

## Inicio rápido: levanta el proyecto en 10 minutos

### 1. Requisitos

Instala antes de comenzar:

| Herramienta | Versión esperada | Uso |
|---|---:|---|
| .NET SDK | 10 | API, EF Core y pruebas backend |
| Node.js | 20 o superior | Frontend React/Vite |
| npm | Incluido con Node.js | Dependencias frontend |
| Git | Cualquier versión reciente | Trabajo incremental |

SQLite es el motor predeterminado y **no requiere instalar un servidor de base de datos**.

Verifica tu ambiente:

```powershell
dotnet --version
node --version
npm --version
git --version
```

### 2. Restaurar, compilar y probar backend

Desde la raíz del repositorio (`Taqueria`):

```powershell
dotnet restore
dotnet build
dotnet test
```

Inicia la API:

```powershell
dotnet run --project src\Taqueria.Api
```

La API se expone en:

```text
http://localhost:5082
http://localhost:5082/swagger
```

Al primer arranque, la API aplica las migraciones pendientes y carga usuarios/datos demo si están habilitados.

### 3. Instalar y ejecutar frontend

En otra terminal:

```powershell
cd src\Taqueria.Web
npm ci
npm run dev
```

Abre:

```text
http://localhost:5173
```

Vite redirige las llamadas `/api` hacia `http://localhost:5082`; por eso API y frontend deben estar ejecutándose al mismo tiempo.

### 4. Primer inicio de sesión

| Usuario | Contraseña temporal | Rol operativo |
|---|---|---|
| `admin` | `admin123` | Administración general |
| `mesero` | `mesero123` | Captura, entrega y envío a caja |
| `cocina` | `cocina123` | Preparación y liberaciones parciales |
| `caja` | `caja123` | Cobro, liberación de mesa y corte |

Las credenciales iniciales son temporales. La primera vez que inicies sesión deberás definir una contraseña nueva antes de operar.

---

## Qué hace actualmente el sistema

Estado funcional: **ZIP-007 RC2 — Merma y Pago Mixto**, sobre la base estable **ZIP-006A RC1**.

### Catálogos y administración

- Administración de usuarios y roles (`Administrador`, `Mesero`, `Cocina`, `Caja`).
- Activación/desactivación de usuarios sin eliminar su historial.
- Restablecimiento de contraseña temporal y cambio obligatorio al iniciar sesión.
- CRUD de mesas, categorías y productos.
- Histórico de precios por producto: cambiar un precio no altera ventas anteriores.

### Operación del restaurante

```text
Mesero abre mesa y captura pedido por comensal
        ↓
Cocina recibe partidas y libera cantidades parciales
        ↓
Mesero confirma entregas a la mesa y envía la cuenta
        ↓
Caja cobra con uno o varios métodos de pago y libera la mesa
```

- Una mesa sólo puede tener una comanda activa.
- La captura utiliza catálogo; no se escribe el nombre del platillo manualmente.
- Las partidas guardan nombre, categoría y precio históricos.
- Cocina puede liberar, por ejemplo, `20 + 15 + 15` unidades de una partida de `50`.
- Caja puede cobrar con efectivo, tarjeta, transferencia o pago mixto.
- El total nunca se captura manualmente: siempre se calcula desde las partidas históricas.

### Cancelaciones y merma

- El mesero puede cancelar su comanda antes de enviarla a cocina.
- Administración puede cancelar una orden enviada a cocina mientras no haya entrega confirmada.
- Si ya existe producto preparado pero no entregado, la cancelación genera registros de **merma** por partida.
- Si ya hubo entrega o cobro, el sistema rechaza la cancelación: ese caso requiere un flujo futuro de devolución o anulación.

La merma se valora al precio histórico de referencia de la partida. Aún no es costo de producción, porque el MVP no modela recetas ni inventario de insumos.

### Reportes

Caja y Administración pueden consultar:

- Ventas cobradas del día.
- Ventas por producto.
- Ventas por categoría histórica.
- Ventas por mesero.
- Totales por método de pago, incluyendo distribuciones mixtas.
- Mermas del día.
- Corte Excel `Ventas_YYYY_MM_DD.xlsx`, generado con ClosedXML.

---

## Arquitectura para aprender y mantener

La solución es un **monolito modular**. No utiliza microservicios, CQRS, Mediator ni AutoMapper: las reglas importantes deben poder seguirse leyendo el código y usando el depurador.

```text
src/
  Taqueria.Domain          Entidades, enums, reglas de negocio e interfaces de repositorio/UoW
  Taqueria.Application     DTOs, interfaces de servicios, servicios y mappers explícitos
  Taqueria.Infrastructure  EF Core, SQLite/SQL Server factories, repositorios, JWT, seed y Excel
  Taqueria.Api             Controllers, middleware, Swagger y composición de dependencias
  Taqueria.Web             React 19 + TypeScript + Vite + Bootstrap + Fluent UI

tests/
  Taqueria.Domain.Tests            Reglas puras del negocio
  Taqueria.Application.Tests       Servicios y validaciones de casos de uso
  Taqueria.Api.IntegrationTests    Recorridos HTTP contra la API
```

### Reglas de separación

| Capa | Puede conocer | No debe conocer |
|---|---|---|
| `Domain` | Entidades y reglas del negocio | EF Core, HTTP, React, DTOs |
| `Application` | DTOs, servicios, entidades internamente, repositorios abstractos | Controllers, SQLite concreto |
| `Infrastructure` | EF Core, repositorios, almacenamiento, seguridad y exportación | UI |
| `Api` | DTOs, contratos de servicios y autenticación HTTP | Entidades en controllers |
| `Web` | Contratos HTTP y experiencia de usuario | EF Core o reglas persistentes |

### Patrón de lectura recomendado para un estudiante

Para comprender una funcionalidad, sigue una sola acción de extremo a extremo. Ejemplo: **enviar una cuenta a Caja**.

1. Localiza la pantalla en `src/Taqueria.Web/src/pages/ComandasPage.tsx`.
2. Busca la llamada HTTP en la pantalla o en `services/apiClient.ts`.
3. Abre el endpoint correspondiente en `src/Taqueria.Api/Controllers/ComandasController.cs`.
4. Sigue la interfaz e implementación del servicio en `Taqueria.Application`.
5. Observa qué método de la entidad `Comanda` aplica la regla.
6. Revisa cómo el repositorio carga el agregado y cómo `IUnitOfWork` confirma los cambios.
7. Busca la prueba que demuestre el comportamiento esperado.

Ese recorrido muestra las responsabilidades sin necesidad de entender toda la solución al mismo tiempo.

---

## Configuración del ambiente de desarrollo

Los archivos principales son:

```text
src/Taqueria.Api/appsettings.json
src/Taqueria.Api/appsettings.Development.json
src/Taqueria.Api/Properties/launchSettings.json
src/Taqueria.Web/vite.config.ts
```

### SQLite predeterminado

En desarrollo, la configuración usa SQLite y genera automáticamente la base fuera del repositorio:

| Sistema | Ubicación esperada |
|---|---|
| Windows | `%LocalAppData%\Taqueria\taqueria.dev.db` |
| Linux/macOS | Directorio local de datos del usuario, normalmente `~/.local/share/Taqueria/taqueria.dev.db` |
| Azure App Service | `%HOME%/Data/Taqueria/taqueria.db` |

Esto evita que una publicación, limpieza de Git o reemplazo del directorio de la aplicación destruya la información del POS.

Configuración relevante:

```json
{
  "Database": {
    "Provider": "SQLite",
    "ConnectionString": "",
    "SQLite": {
      "FileName": "taqueria.db",
      "StorageLocation": "Auto",
      "CustomDataDirectory": "",
      "DefaultTimeoutSeconds": 60,
      "EnableWriteAheadLogging": true,
      "EnableWriteAheadLoggingOnAzureHome": false
    }
  }
}
```

`appsettings.Development.json` cambia el nombre a `taqueria.dev.db` para separar datos locales de configuraciones de publicación.

### Usar una carpeta SQLite personalizada

Para una prueba controlada puedes configurar una carpeta propia en `appsettings.Development.json`:

```json
{
  "Database": {
    "SQLite": {
      "StorageLocation": "Custom",
      "CustomDataDirectory": "C:\\Datos\\Taqueria",
      "FileName": "taqueria.dev.db"
    }
  }
}
```

Valores soportados en `StorageLocation`:

```text
Auto
LocalApplicationData
AzureHome
Custom
```

### Hardening SQLite

En almacenamiento local, el sistema activa:

- Llaves foráneas.
- Connection pooling.
- Timeout de bloqueo de 60 segundos.
- `journal_mode=WAL` para reducir contención entre lecturas y escrituras.

En Azure App Service, WAL permanece deshabilitado por defecto porque `%HOME%` puede estar sobre almacenamiento compartido. SQLite debe utilizarse allí sólo en una instancia ligera. Para escalar horizontalmente, la evolución correcta es SQL Server o Azure SQL.

### Reiniciar la base de desarrollo

Detén primero la API. Después ejecuta, desde la raíz:

```powershell
.\scripts\reset-sqlite.ps1
```

En el siguiente arranque se recrea la base local, se aplican las migraciones y se vuelven a insertar los datos seed.

> No borres la carpeta `src/Taqueria.Infrastructure/Persistence/Migrations` en una instalación normal. Esa carpeta es código fuente y debe permanecer versionada. Sólo se eliminó una vez durante la consolidación histórica del RC1.

### JWT en desarrollo

La configuración local incluye una llave de desarrollo para facilitar el arranque. Antes de publicar una aplicación real:

- Configura la llave JWT mediante secreto de ambiente o proveedor seguro.
- Usa HTTPS.
- Cambia todas las credenciales temporales.
- Revisa expiración, emisión y estrategia de autenticación para el entorno de producción.

### SQL Server Express opcional

La infraestructura contiene factories para seleccionar proveedor sin modificar controllers ni servicios. Antes de operar con SQL Server, configura una cadena válida y genera/valida migraciones específicas para ese proveedor; las migraciones SQLite del RC no deben asumirse como despliegue SQL Server ya validado.

---

## Datos demo y recorrido de prueba manual

Con `Seed:LoadDemoCatalog = true`, una base nueva incluye categorías, productos, precios y mesas de demostración.

### Recorrido completo de una venta

1. Inicia sesión como `mesero` y cambia su contraseña temporal.
2. Abre una mesa libre desde **Comandas**.
3. Agrega comensales y productos; verifica que el total se calcule automáticamente.
4. Envía el pedido a Cocina.
5. Inicia sesión como `cocina`, cambia contraseña y libera cantidades parciales.
6. Regresa como `mesero`, confirma cada entrega a la mesa y envía la cuenta a Caja.
7. Inicia sesión como `caja`, cambia contraseña y registra un pago simple o mixto.
8. Confirma que la mesa vuelve a quedar libre.
9. Entra como `admin` o `caja` a **Reportes** y descarga el corte Excel.

### Recorrido de merma

1. Como mesero, abre una mesa y envía una partida a Cocina.
2. Como cocina, libera una cantidad, pero no la entregues al cliente.
3. Como administrador, cancela la comanda indicando un motivo.
4. En Reportes, verifica las unidades e importe histórico registrados como merma.

---

## Pruebas automáticas

Ejecuta todas las pruebas backend desde la raíz:

```powershell
dotnet test
```

Ejecuta también el build del frontend:

```powershell
cd src\Taqueria.Web
npm ci
npm run build
```

### Qué valida cada proyecto

| Proyecto | Ejemplos de reglas cubiertas |
|---|---|
| `Taqueria.Domain.Tests` | Precio histórico, cantidades y cancelación/merma |
| `Taqueria.Application.Tests` | Validaciones de servicios, como mesas duplicadas |
| `Taqueria.Api.IntegrationTests` | Login, catálogos, flujo de comanda, cocina, caja, reportes y usuarios |

Antes de aceptar un cambio funcional, la disciplina mínima del repositorio es:

```powershell
dotnet build
dotnet test
cd src\Taqueria.Web
npm run build
```

---

## Rutas de la aplicación web

| Ruta | Función | Roles principales |
|---|---|---|
| `/login` | Inicio de sesión | Todos |
| `/cambiar-password` | Sustituir contraseña temporal | Todos autenticados |
| `/mesas` | Administrar mesas | Administrador |
| `/catalogos` | Productos, categorías y precios | Administrador |
| `/usuarios` | Usuarios, roles y accesos | Administrador |
| `/comandas` | Abrir mesa, capturar y entregar pedido | Mesero / Administrador |
| `/cocina` | Liberar preparación parcial | Cocina / Administrador |
| `/caja` | Registrar pago y liberar mesa | Caja / Administrador |
| `/reportes` | Ventas, mermas y Excel | Caja / Administrador |

Swagger documenta los endpoints HTTP disponibles en:

```text
http://localhost:5082/swagger
```

---

## Problemas frecuentes

### El frontend muestra `Cannot find module ... node_modules\vite\bin\vite.js`

Las dependencias locales están incompletas. Desde `src\Taqueria.Web`:

```powershell
Remove-Item .\node_modules -Recurse -Force -ErrorAction SilentlyContinue
npm ci
npm run dev
```

### La UI no puede llamar a `/api/...`

Confirma que la API esté ejecutándose en `http://localhost:5082` antes de iniciar o probar Vite. El proxy se encuentra en `src/Taqueria.Web/vite.config.ts`.

### Quiero empezar de cero con datos demo

Detén la API y ejecuta:

```powershell
.\scripts\reset-sqlite.ps1
dotnet run --project src\Taqueria.Api
```

### SQLite reporta archivo ocupado

Cierra la API y cualquier herramienta que tenga abierta la base. Recuerda que WAL puede crear archivos auxiliares `-wal` y `-shm`; el script de reinicio los elimina junto con la base local.

---

## Trabajo con Git

El proyecto nació mediante ZIPs incrementales para poder estudiar diferencias. En el repositorio actual, trabaja normalmente con commits pequeños y verificables:

```powershell
git status
git add .
git commit -m "Describe el cambio realizado"
```

Antes de una nueva funcionalidad:

1. Confirma que el proyecto compila y las pruebas pasan.
2. Crea un commit de baseline estable.
3. Implementa un cambio de responsabilidad limitada.
4. Agrega o actualiza pruebas.
5. Repite build y pruebas antes del commit final.

---

## Próximas evoluciones razonables

- Devoluciones o anulaciones posteriores a entrega/cobro.
- Bitácora administrativa de acciones sensibles.
- Inventario y recetas para costear merma en lugar de valorarla sólo a precio histórico.
- Validación formal de despliegue con SQL Server/Azure SQL.
- Impresión de ticket o integración con impresora térmica.

Este MVP prioriza una operación real entendible: cada cambio de estado debe tener un responsable, cada cobro debe cuadrar con el pedido histórico y cada pérdida preparada debe quedar explicada.
