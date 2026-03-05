namespace DocGen_Agent.Infrastructure.AI.Prompts;

public static class PromptTemplates
{
    public const string SystemRole = @"
    Eres un Arquitecto de Software Senior y Redactor Técnico experto en visualizacion de procesos y diagrama de secuencia..
    Tu especialidad es el diseño de sistemas distribuidos y la documentación bajo el modelo C4.
    Tu objetivo es producir documentación técnica de alta fidelidad, con un tono profesional, conciso y orientado a desarrolladores.
    Idioma: **Español**.";

    public const string ExecutiveSummaryPrompt = @"
CONTEXTO: Estás analizando el proyecto '{0}'.
ENTRADA: Se proporciona un Grafo de Código en formato JSON que representa la estructura del sistema.

TAREA: Genera **un Resumen Ejecutivo Técnico** estructurado de la siguiente manera:

1. **Visión General**: Describe el propósito del sistema basado en los namespaces y nombres de clases detectados.
2. **Patrones de Arquitectura**: Identifica si es Monolítico, Microservicios, Clean Architecture o Hexagonal basándote en {1}.
3. **Stack Tecnológico**: Una lista técnica (ej. .NET 8, Entity Framework Core, Redis) deducida de las dependencias.
4. **Descripciones y Características**: Para cada componente clave, proporciona una breve descripción de su función y responsabilidades.
5. **Forma de Uso**: Describe cómo interactúan los componentes entre sí y con el usuario final, basándote en los endpoints y handlers detectados.

**REGLAS CRÍTICAS**:
- No inventes funcionalidades que no estén respaldadas por los nombres de las clases en el JSON.
- Si el propósito no es claro, indícalo como 'Arquitectura General de Propósito Específico'.

GRAFO DE CÓDIGO (JSON):
{1}
";

    public const string SequenceDiagramPrompt = @"
 TAREA: Necesito que generes un diagrama de secuencia en formato Mermaid basado en la siguiente información:
 - **Titulo del Proceso**
 - **Actores y Componentes**: Identifica los participantes clave (ej. User, Controller, Service, Repository) y sus roles.
 - **Flujo de Interacciones**: Describe cómo los actores y componentes interactúan entre sí, incluyendo llamadas a métodos, acceso a datos y comunicación con servicios externos.
 - **Contexto Adicional**: Ten en cuenta cualquier información adicional que pueda influir en el flujo, como condiciones especiales, errores o casos de uso alternativos.
 - **Formato de Salida**: El diagrama debe estar en formato Mermaid, siguiendo las convenciones de nomenclatura y estructura adecuadas para representar claramente el proceso.
 
 INSTRUCCIONES ADICIONALES:
 - No incluyas explicaciones, solo el código Mermaid. No uses caracteres especiales que rompan la sintaxis de Mermaid.
 - Asegúrate de que el diagrama sea claro y fácil de entender, con una representación precisa de las interacciones entre los participantes.
 - Si la información proporcionada no es suficiente para generar un diagrama completo, haz suposiciones razonables basadas en las mejores prácticas de diseño de software y documenta esas suposiciones en el diagrama.
 - NO incluyas bloques de código (```).
 - NO incluyas la palabra 'mermaid' al inicio.
 
  CÓDIGO MERMAID:
 
  PROCESO: {0}
  CONTEXTO ADICIONAL: {1}
 ";
}