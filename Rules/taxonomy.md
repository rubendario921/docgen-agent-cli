# Taxonomía

- Controlador: punto de entrada HTTP (ASP.NET Core).
- Servicio: lógica de aplicación (si se detecta).
- Repositorio: acceso a datos (si se detecta).
- DTO: contratos de entrada/salida (si se detecta).
- Endpoint: método HTTP + ruta + handler.

## Angular (Frontend)

- **ng-module**: clase decorada con `@NgModule`.
- **ng-component**: clase decorada con `@Component` (selector, template, estilos).
- **ng-service**: clase decorada con `@Injectable`, suele orquestar llamadas HTTP.
- **ng-route**: entrada de ruta definida en `RouterModule.forRoot/forChild`.
- **Rutas de UI**: se documentan como `ROUTE /path` → `Componente.render`.
- **Llamadas HTTP**: se documentan como `HTTP.<VERB> <URL>` desde servicios. Si la URL no es literal, se marca como _No identificado..._.