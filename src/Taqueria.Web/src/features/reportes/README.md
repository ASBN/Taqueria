# Feature Reportes

Implementada desde version-005.

Permisos: `Caja` y `Administrador`.

La pantalla consulta ventas cobradas y mermas por fecha local, muestra indicadores y agrupados, y descarga `Ventas_YYYY_MM_DD.xlsx` desde la API. El Excel incorpora la hoja `Mermas`. El navegador envía su desfase horario para que el corte respete el día operativo local aun cuando la base guarde tiempos UTC.
