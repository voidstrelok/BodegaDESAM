# Auditoría pre-producción: Bodega DESAM

- **Fecha:** 2026-10-15
- **Alcance:** código (.NET 8 / Blazor Server / EF Core / PostgreSQL), Docker, paquetes, pruebas y demo en https://demobodega.thepit.cl/
- **Contexto confirmado:** producción irá en **un servidor nuevo**; ~**10 usuarios**, **bajo movimiento**.
- **Estado:** solo diagnóstico, no se modificó código. Las 10 pruebas existentes pasan.

**Conclusión:** el núcleo funcional está sólido (roles, multi-bodega, auditoría, validación de stock, restricciones de borrado, migraciones). Los bloqueantes están en seguridad, operación e integridad bajo concurrencia, no en funcionalidad.

---

## Efecto del contexto (servidor nuevo, 10 usuarios)

- **Servidor nuevo:** se despliega desde cero, por lo que se puede hacer bien desde el inicio: red interna de Docker sin puertos de BD, proxy inverso con TLS configurado antes de abrir, backups y monitoreo desde el día 1, y base limpia sin las cuentas seed.
- **Migrar datos del demo:** decidir si el demo es solo de prueba (base nueva) o si hay datos a conservar (`pg_dump`/`pg_restore`). Si se conservan, eliminar antes las 3 cuentas seed.
- **10 usuarios / bajo movimiento:**
  - #7 (concurrencia): basta una transacción `Serializable` con reintento. No hace falta nada más complejo.
  - Rendimiento (P2): baja prioridad. No optimizar antes de tener datos reales.
  - `SecurityStampValidator` con intervalo 0 es aceptable a esta escala; se puede dejar.
  - Una sola instancia de la app: la carrera de migraciones al arrancar (#13) es de bajo riesgo, pero conviene dejarla explícita.
  - Backups diarios son suficientes.

---

## P0: Bloqueantes (no lanzar sin resolver)

| # | Hallazgo | Evidencia | Cambio |
|---|---|---|---|
| 1 | **Credenciales conocidas se crean en producción.** `IdentitySeed` corre siempre y crea `rcontreras@desam / Admin123`, `fcortes@desam / Bodega123`, `fespinosa@desam / Lectura123`. | `BodegaDESAM/Data/IdentitySeed.cs:25-27`, `Program.cs:118` | Quitar usuarios hardcodeados. Crear solo un admin inicial desde variables de entorno (`ADMIN_EMAIL`, `ADMIN_PASSWORD`), y solo si no existe ningún admin. En el demo actual, cambiar o eliminar esas cuentas. |
| 2 | **Paquete con vulnerabilidad High:** `Microsoft.AspNetCore.Components.WebAssembly.Server` 8.0.23 (GHSA-qjpx-jqm5-h2c4). También avisos High en `System.Security.Cryptography.Xml` y `Pkcs` (transitivos). | `dotnet list package --vulnerable` | Se resuelve con #3. Además esa referencia es innecesaria: no existe proyecto cliente WASM. |
| 3 | **.NET 8 termina soporte el 10-nov-2026.** | `BodegaDESAM.csproj`, `Dockerfile` | Migrar a .NET 10 LTS: TFM, imágenes `sdk:10.0`/`aspnet:10.0`, paquetes EF/Npgsql/Identity 10.x. Probar migraciones y PDFs. Hacerlo antes de desplegar en el servidor nuevo. |
| 4 | **Cookies sin `Secure` y sin HSTS en el demo.** La cookie antiforgery sale sin `secure`. Los `ForwardedHeaders` por defecto solo confían en loopback; tras nginx/Docker se ignora `X-Forwarded-Proto` y la app cree que es HTTP. Probable que la cookie de sesión tenga el mismo problema. | `curl` a `/login`; `Program.cs:128-134` | Limpiar `KnownNetworks`/`KnownProxies` (o fijar la red de Docker). `CookieSecurePolicy.Always` en cookie de Identity y antiforgery. Verificar que HSTS llegue al cliente. |
| 5 | **Postgres expuesto al host** (`5435:5432`). | `docker-compose.yml:16-17` | Eliminar `ports` de `db`. La red interna de compose basta. |
| 6 | **Sin respaldos.** Datos solo en el volumen `postgres_data`; sin `pg_dump` programado ni restore probado. | `docker-compose.yml` | Backup diario con retención y copia fuera del servidor; restore de prueba documentado. Respaldar también el volumen `data_protection_keys`. |
| 7 | **Race conditions en stock.** `ValidarStockDisponibleAsync` lee y suma sin transacción y luego inserta; dos salidas simultáneas contra la misma entrada pasan ambas la validación. Igual en ajustes de disminución. Sin `BeginTransaction`, `xmin` ni tokens de concurrencia. | `Services/SalidaService.cs:111-123`, `Services/AjusteInventarioService.cs:86-117` | Envolver validación + guardado en transacción `Serializable` (o `SELECT ... FOR UPDATE` sobre `detalle_entrada` de origen) y reintentar ante `40001`. Agregar `xmin` como token de concurrencia en entidades editables. |

## P1: Alta prioridad (antes del go-live o primera semana)

| # | Hallazgo | Cambio |
|---|---|---|
| 8 | **Autorización por bodega solo en la UI.** `SalidaService`/`EntradaService`/`AjusteInventarioService` (`DeleteAsync`, `UpdateAsync`) no verifican acceso a la bodega del registro. Hoy lo cubre el filtro de bodega activa de las páginas; cualquier ruta nueva lo rompe. | Verificar `BodegaAuthorizationService.PuedeAccederAsync` dentro del servicio, o pasar el `idBodega` permitido y filtrar por él. |
| 9 | **Auditoría incorrecta y con fallos silenciosos.** `SalidaService.UpdateAsync/DeleteAsync` y `EntradaService.DeleteAsync` registran el `IdUsuario` del creador, no de quien edita/borra. `AuditService` traga toda excepción sin loguear. | Pasar el usuario actual (existe `AuditContextService`). Loguear fallos con `ILogger`. Guardar el detalle de lo eliminado (producto, cantidades). |
| 10 | **Sin CI y pruebas fuera de git.** `.gitignore` excluye `BodegaDESAM.Tests/`, `.github/`, `tools/`, `docs/`. Solo 10 pruebas (paginación, usuario-bodega, valorización); ninguna de salidas, ajustes ni stock. | Versionar el proyecto de pruebas. Pruebas de stock (incluida concurrencia con Postgres real, ej. Testcontainers). Workflow de GitHub Actions: build, test, `dotnet list package --vulnerable`. |
| 11 | **`.vs/` rastreado en git** (2.263 archivos, incl. `CopilotSnapshots`) pese a estar en `.gitignore`. Pueden contener copias de código. | `git rm -r --cached .vs`. Revisar si algún snapshot contiene secretos. |
| 12 | **Errores internos mostrados al usuario.** 21 usos de `ex.Message`/`catch (Exception` en páginas; filtran mensajes de Npgsql/EF. | Capturar `InvalidOperationException` de negocio con texto controlado; para el resto, mensaje genérico + log. |
| 13 | **Migraciones al arrancar sin control.** `MigrateAsync` corre siempre; `Database__ApplyMigrationsOnStartup` del compose no se lee en ningún lado. | Leer la variable realmente, o ejecutar migraciones como paso de despliegue (`dotnet ef migrations bundle`). |
| 14 | **Observabilidad nula.** Sin logging estructurado, monitoreo ni alertas; `/health` público. | Serilog a stdout/archivo con rotación. Monitoreo externo de `/health`. Revisar qué expone `/health` hacia afuera. |
| 15 | **Licencia QuestPDF.** `LicenseType.Community` con v2024.3.0. | Confirmar que DESAM califica bajo las condiciones de ingresos; si no, licencia comercial o cambiar de librería. |

## P2: Importante, planificable

- **Código muerto/demo expuesto:** `Weather.razor` y `EjemploTema.razor` son públicas sin `[Authorize]`; `Users.razor` conserva ~230 líneas de vista legacy (`mostrarVistaLegacy = false`); WASM declarado (`AddInteractiveWebAssemblyComponents`, `WebAssembly.Server`) sin proyecto cliente. Eliminar.
- **Login:** hay lockout; agregar rate limiter por IP (`AddRateLimiter`) y política de contraseñas más fuerte (hoy 8 caracteres, sin símbolos). No hay recuperación de contraseña (solo reset por admin): definir si se necesita.
- **Cabeceras de seguridad:** solo llega `X-Frame-Options`. Agregar CSP, `X-Content-Type-Options`, `Referrer-Policy`.
- **Sesión:** definir expiración de cookie y sliding expiration. Intervalo 0 de `SecurityStampValidator` se puede dejar a esta escala.
- **Dockerfile / Compose:** correr como no-root (`USER app`), `HEALTHCHECK`, fijar versiones de imagen, límites de memoria, rotación de logs, secretos fuera del `.env` plano (o `chmod 600`).
- **Dos registros de `DbContext`** (`AddDbContextFactory` + `AddDbContext`): unificar.
- **Rendimiento (baja prioridad a esta escala):** `GetAllAsync` con múltiples `Include` (15 usos), agregados en memoria en `AjusteInventarioService`, N+1 en `Users.razor`, `Include` profundo en `SalidaService.GetByIdAsync`; revisar índices en `id_bodega`, `id_producto`, `id_detalle_entrada`. Medir con datos reales antes de optimizar.

## P3: Mejoras de producto

- Anulación con motivo en lugar de borrado físico de entradas/salidas/ajustes (trazabilidad).
- Exportación a Excel y reportes de inventario/valorización.
- Configuración del sistema (nombre de la institución, logo en PDFs).
- Gestión de sesiones activas y cierre forzado.
- Versionar el manual operativo (`docs/` está ignorado).
- Revisión de accesibilidad y uso móvil en pantallas principales.

---

## Plan sugerido por fases

1. **Seguridad rápida:** #1, #4, #5, #11, borrar páginas demo.
2. **Plataforma:** #3 + #2 (migración a .NET 10), #13, #15.
3. **Integridad:** #7 con sus pruebas (#10), luego #8 y #9.
4. **Operación (servidor nuevo):** #6, #14, CI, Dockerfile/Compose, resto de P1.
5. **Post-lanzamiento:** P2 y P3 según uso real.

## Checklist de salida a producción

- [ ] Sin cuentas seed; admin inicial creado por variables de entorno
- [ ] .NET 10 y `dotnet list package --vulnerable` sin hallazgos High
- [ ] TLS + HSTS + cookies `Secure` verificados con `curl -I`
- [ ] Puerto de Postgres no accesible desde fuera
- [ ] Backup diario ejecutado y restore probado
- [ ] Prueba de concurrencia de salidas simultáneas aprobada
- [ ] Monitoreo de `/health` activo
- [ ] `.vs/` fuera de git; pruebas versionadas; CI verde
- [ ] Páginas demo (`Weather`, `EjemploTema`) eliminadas

## Decisiones tomadas

- Es posible migrar datos desde el demo: antes de restaurar, eliminar las 3 cuentas seed (`*@desam`) de la tabla de usuarios.
- Las contraseñas las administra el admin (sin recuperación por correo ni SMTP).
- DESAM califica para QuestPDF Community: #15 cerrado.

## Avance

**Fase 1 (seguridad rápida), implementada en el árbol de trabajo, sin commit:**
- #1: las cuentas seed solo se crean en Development. En producción se crea un admin inicial desde `ADMIN_EMAIL`/`ADMIN_PASSWORD` (`Admin__Email`/`Admin__Password`) únicamente si no existe ningún Admin.
- #4: `ForwardedHeaders` confía en el proxy (listas limpiadas) y las cookies de Identity y antiforgery son `Secure` fuera de Development. Verificar con `curl -I` tras desplegar. HSTS lo emite la app; confirmar que el proxy no lo elimine.
- #5: Postgres sin puerto publicado; la app escucha solo en `127.0.0.1:3100` (el proxy debe estar en el mismo host; si no, ajustar).
- #11: `.vs/` sacado del índice de git (`git rm --cached`); los archivos locales siguen intactos.
- P2: eliminadas `Weather.razor` y `EjemploTema.razor`.

**Pendiente:** el resto de P0/P1 (#2/#3 migración a .NET 10, #6, #7, #8, #9, #10, #12, #13, #14).
