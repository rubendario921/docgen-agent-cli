using DocGen_Agent.Core.Abstractions;
using DocGen_Agent.Core.Models;
using DocGen_Agent.Infrastructure.AI.Providers;
using DocGen_Agent.Infrastructure.Git;
using DocGen_Agent.Infrastructure.Render;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace DocGen_Agent.Cli.Commands;

public static class RenderCommand
{
    public static Command Build()
    {
        var cmd = new Command("render", "Renderiza Markdown desde un grafo de código");
        var graph = new Option<string>("--graph") { IsRequired = true };
        var templates = new Option<string>("--templates", () => "Templates", "Carpeta de plantillas Scriban");
        var rules = new Option<string>("--rules", () => "Rules", "Carpeta de reglas");
        var project = new Option<string>("--project-name") { IsRequired = true };
        var outMd = new Option<string>("--out") { IsRequired = true };
        var _agentType = new Option<string>("--agent-type", "Tipo de agente (OpenAI, Copilot, Gemini)");
        var _aiEndPoint = new Option<string>("--ai-endpoint", "EndPoint de los modelos disponibles");
        var _aiKey = new Option<string>("--ai-key", "Token de acceso para consumo del agente");
        var _aiModel = new Option<string>("--ai-model", "Motor de agente IA");

        cmd.AddOption(graph);
        cmd.AddOption(templates);
        cmd.AddOption(rules);
        cmd.AddOption(project);
        cmd.AddOption(outMd);
        cmd.AddOption(_agentType);
        cmd.AddOption(_aiEndPoint);
        cmd.AddOption(_aiKey);
        cmd.AddOption(_aiModel);

        cmd.SetHandler(async (InvocationContext context) =>
        {
            try
            {
                var graphPath = context.ParseResult.GetValueForOption(graph)!;
                if (!File.Exists(graphPath)) throw new FileNotFoundException($"No se encontró el grafo en: {graphPath}");

                var tplDir = context.ParseResult.GetValueForOption(templates)!;
                var rulesDir = context.ParseResult.GetValueForOption(rules);
                var projectName = context.ParseResult.GetValueForOption(project)!;
                var outPath = context.ParseResult.GetValueForOption(outMd)!;

                var agentType = context.ParseResult.GetValueForOption(_agentType);
                if (string.IsNullOrWhiteSpace(agentType)) throw new ArgumentException("Tipo de agente no válido");

                var endpoint = context.ParseResult.GetValueForOption(_aiEndPoint);
                if (string.IsNullOrWhiteSpace(endpoint)) throw new ArgumentException("Endpoint no válido");

                var key = context.ParseResult.GetValueForOption(_aiKey);
                if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key no válida");

                var model = context.ParseResult.GetValueForOption(_aiModel);
                if (string.IsNullOrWhiteSpace(model)) throw new ArgumentException("Model no válido");

                var json = await File.ReadAllTextAsync(graphPath);
                var codeGraph = JsonSerializer.Deserialize<CodeGraph>(json)
                                ?? throw new InvalidOperationException("El archivo de grafo no es un JSON válido o está vacío.");

                var tplMainContent = ReadTemplateContent(tplDir, "main.sbn");
                var tplSeqContent = ReadTemplateContent(tplDir, "sequence.sbn", "```mermaid\nsequenceDiagram\nparticipant Client\nparticipant API\nClient->>API: Request\nAPI-->>Client: Response\n```");

                IAIService aiService;

                aiService = agentType.ToLower() switch
                {
                    "azure" => new AzureOpenAIService(endpoint, key, model),
                    "copilot" => new GitHubCopilotService(endpoint, key, model),
                    "gemini" => new GeminiService(endpoint, key, model),
                    _ => throw new ArgumentException("Tipo de agente no válido"),
                };

                Console.WriteLine($"[docgen] Iniciando enriquecimiento con agente {agentType}...");
                var viewModel = await CreateViewModelAsync(codeGraph, projectName, tplSeqContent, aiService);

                Console.WriteLine($"[docgen] Generando diagramas de secuencia...");
                var renderer = new ScribanRenderer();
                var md = renderer.Render(tplMainContent, viewModel);

                Console.WriteLine($"[docgen] Renderizado completo. Generando Markdown...");
                var outDir = Path.GetDirectoryName(outPath);
                if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);

                await File.WriteAllTextAsync(outPath, md);

                Console.WriteLine($"[docgen] Markdown generado exitosamente en: {outPath}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[critical] Fallo en la renderización: {ex.Message}");
            }
        });

        return cmd;
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
                var endpointsToProcess = codeGraph.Endpoints.Take(3).ToList();
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
            change_history = GitHistoryReader.ReadLastChanges(10),
            commit_sha = Environment.GetEnvironmentVariable("BUILD_SOURCEVERSION") ?? "N/A",
            generated_at = DateTime.UtcNow.ToString("u")
        };
    }

    private static string ReadTemplateContent(string tplDir, string fileName, string? fallback = null)
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
        var assembly = typeof(RenderCommand).Assembly;
        var resourceName = $"DocGen_Agent.Templates.{fileName}";
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream != null)
        {
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        if (fallback != null) return fallback;

        throw new FileNotFoundException($"No se encontró plantilla '{fileName}' ni en disco ni en recursos embebidos.", path);
    }
}