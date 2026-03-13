using DocGen_Agent.Application.Ports;
using DocGen_Agent.Domain.Entities;
using DocGen_Agent.Domain.Exceptions;
using DocGen_Agent.Infrastructure.Git;

namespace DocGen_Agent.Application.UseCases.RenderApplication;

public class RenderUseCase : IRenderUseCase
{
    private readonly IRenderer _renderer;
    private readonly IAIServiceFactory _aiServiceFactory;

    public RenderUseCase(IRenderer renderer, IAIServiceFactory aiServiceFactory)
    {
        _renderer = renderer;
        _aiServiceFactory = aiServiceFactory;
    }

    public async Task<string> ExecuteAsync(CodeGraph codeGraph, string projectName, string templatesDir, string? rulesDir, string agentType, string aiEndpoint, string aiKey, string aiModel)
    {
        if (codeGraph == null)
            throw new DomainException("El grafo de código no puede ser nulo");

        var tplMainContent = ReadTemplateContent(templatesDir, "main.sbn");
        var tplSeqContent = ReadTemplateContent(templatesDir, "sequence.sbn", "```mermaid\nsequenceDiagram\nparticipant Client\nparticipant API\nClient->>API: Request\nAPI-->>Client: Response\n```");

        IAIService? aiService = _aiServiceFactory.Create(agentType, aiEndpoint, aiKey, aiModel);

        Console.WriteLine($"[docgen] Iniciando enriquecimiento con agente {agentType}...");
        var viewModel = await CreateViewModelAsync(codeGraph, projectName, tplSeqContent, aiService);

        Console.WriteLine($"[docgen] Generando diagramas de secuencia e inyectando IA...");
        return _renderer.Render(tplMainContent, viewModel);
    }

    private string ReadTemplateContent(string tplDir, string fileName, string? fallback = null)
    {
        var path = Path.Combine(tplDir, fileName);
        if (File.Exists(path)) return File.ReadAllText(path);

        // Fallback a carpeta lowercase (compatibilidad Linux)
        var pathLower = Path.Combine(tplDir.ToLowerInvariant(), fileName);
        if (File.Exists(pathLower)) return File.ReadAllText(pathLower);

        // Fallback a carpeta local 'Templates'
        var localPath = Path.Combine("Templates", fileName);
        if (File.Exists(localPath)) return File.ReadAllText(localPath);

        // Fallback a Embedded Resource
        var assembly = typeof(RenderUseCase).Assembly;
        var resourceName = $"DocGen_Agent.Templates.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        if (fallback != null) return fallback;

        throw new DomainException($"No se encontró plantilla '{fileName}' ni en disco ni en recursos embebidos.");
    }

    private static async Task<object> CreateViewModelAsync(CodeGraph codeGraph, string projectName, string tplSeqContent, IAIService? aiService)
    {
        var executiveSummary = "_(Generado automáticamente; se enriquecerá en Fase 3 con IA)_";
        var sequenceDiagrams = tplSeqContent;

        if (aiService != null)
        {
            try
            {
                executiveSummary = await aiService.EnrichSectionAsync("ExecutiveSummary", codeGraph, projectName);

                var sb = new System.Text.StringBuilder();
                var endpointsToProcess = codeGraph.Endpoints.Take(10).ToList();
                foreach (var ep in endpointsToProcess)
                {
                    var diagram = await aiService.GenerateSequenceDiagramAsync(ep, $"Project: {projectName}, Framework: {codeGraph.Framework}");
                    sb.AppendLine($"### Flujo: {ep.Method} {ep.Path}");
                    sb.AppendLine(":::mermaid");
                    sb.AppendLine(diagram);
                    sb.AppendLine(":::");
                    sb.AppendLine();
                }
                if (sb.Length > 0) sequenceDiagrams = sb.ToString();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[warn] Error en enriquecimiento IA: {ex.Message}. Usando fallbacks.");
            }
        }

        return new
        {
            project_name = projectName,
            executive_summary = executiveSummary,
            sequence_diagrams = sequenceDiagrams,
            change_history = GitHistoryReader.ReadLastChanges(10), // We'll mock/refactor GitHistoryReader in a moment
            commit_sha = Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION") ?? "N/A",
            generated_at = DateTime.UtcNow.ToString("u")
        };
    }
}
