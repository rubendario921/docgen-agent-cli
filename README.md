# DocGen-Agent

---

## 1. Objetivo Principal

Implementar un **pipeline manual y reutilizable** en Azure DevOps que, usando un **Agente CLI .NET publicado como dotnet tool** en un feed privado (Azure Artifacts), **genere documentación técnica en Markdown** para los repos, en un **formato estandarizado**, con publicación **directa a `/docs`** y cargar al **al Wiki** del proyecto.

### Estrategia
Arquitectura Hexagonal, SOLID, Clean Code. Estándar de documentación unificado con plantillas y prompts.

### Resultados esperados

- **Análisis estático** del repositorio (controladores, endpoints, servicios, repos, interfaces, DTOs, módulos, etc).
- **Detección de stack** (lenguaje/framework, dependencias principales).
- **Estructura de documento Markdown** con secciones:
  - Resumen Ejecutivo
  - Arquitectura
  - Stack Tecnológico
  - Descripciones y Características  
  - Diagramas de Secuencia (Mermaid)
  - Historial de los últimos 10 cambios
- **Publicación automática**:
  - Fase 1: commit de `docs/techdoc.md` a la rama que ejecuta el pipeline.
  - Fase 2: publicación adicional en **Azure DevOps Wiki**.
- **Trazabilidad y auditoría** (artefactos con `graph.json`, `techdoc.md`, logs).

---

## 2. Decisiones Estratégicas

- **Herramienta centralizada**: CLI .NET empaquetada como **dotnet tool** para evitar copiar scripts a cada repo.
- **Repositorio por lenguaje**: Soporte inicial **.NET** y **Node (Nest/Express)**.
- **Infraestructura mínima**: Se ejecuta **dentro del job de pipeline** (sin Functions/containers).
- **Control de versiones** del tool vía **Azure Artifacts**.
- **Publicación sin PR**: commit directo a la rama del pipeline (alineado a tu preferencia).
- **Seguridad**: uso de `System.AccessToken` y permisos mínimos.

---

## 3. Arquitectura (Hexagonal)

**Capas (puertos/adaptadores):**

- **Core (Dominio/Aplicación)**
  - **Puertos**:
    - `ISourceCodeScanner` (por lenguaje)
    - `IRenderer` (plantillas)
    - `IPublisher` (no usado en Fase 1 — pipeline publica en repo; Wiki en Fase 2)
  - **Modelos**:
    - `CodeGraph`, `Component`, `Endpoint`
- **Infraestructura (Adaptadores)**
  - **Scanners**:
    - `.NET`: heurística con atributos `[ApiController]`, `[Route]`, `[Http*]` (Roslyn en fases posteriores)
    - **Node**: NestJS (`@Controller`, `@Get|@Post|...`) y Express (`router.method(...)`)
  - **Renderer**:
    - `Scriban` (plantillas `*.sbn`)
  - **Git**:
    - utilitario para `git log -n 10` (historial)
- **CLI (Orquestación)**
  - `docgen scan` → genera `graph.json`
  - `docgen render` → genera `techdoc.md`

**Flujo de alto nivel:**

1. **Pipeline** instala tool →
2. `docgen scan` (detecta stack + construye grafo) →
3. `docgen render` (plantillas + reglas) →
4. **Commit** de `docs/techdoc.md` (sin PR) →
5. **Artefactos** (`graph.json`, `techdoc.md`, logs).
6. _(Fase 2)_ `docgen publish` a Wiki.

---
## 4. Estructura del Repositorio del Agente

docgen-agent/
├─ README.md
├─ global.json
├─ Directory.Build.props
├─ azure-pipelines-docgen-agent.yml # build & publish del tool al feed
├─ templates/
│ ├─ main.sbn
│ └─ sequence.sbn
├─ rules/
│ ├─ formatting.md
│ └─ taxonomy.md
└─ src/
├─ DocGen.Core/
│ ├─ Abstractions/
│ │ ├─ ISourceCodeScanner.cs
│ │ ├─ IRenderer.cs
│ │ └─ IPublisher.cs
│ └─ Models/
│ ├─ CodeGraph.cs
│ ├─ Component.cs
│ └─ Endpoint.cs
├─ DocGen.Infrastructure/
│ ├─ Scanners/
│ │ ├─ DotNetScanner.cs
│ │ └─ NodeScanner.cs
│ ├─ Render/
│ │ └─ ScribanRenderer.cs
│ └─ Git/
│ └─ GitHistoryReader.cs
└─ DocGen.Cli/
├─ DocGen.Cli.csproj # PackAsTool=true, ToolCommandName=docgen
├─ Program.cs
└─ Commands/
├─ ScanCommand.cs
└─ RenderCommand.cs

**Convenciones:**

- **PackageId (dotnet tool):** `AzureDevOps.DocGen.Cli`
- **Feed (Azure Artifacts):** `AzureDevOps`
- **Comando:** `docgen`

---