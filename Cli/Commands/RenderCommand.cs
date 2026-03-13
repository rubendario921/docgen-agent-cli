using DocGen_Agent.Application.Ports;
using DocGen_Agent.Application.UseCases.RenderApplication;
using DocGen_Agent.Domain.Entities;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Text.Json;

namespace DocGen_Agent.Cli.Commands;

public static class RenderCommand
{
    public static Command Build(IRenderUseCase renderUseCase)
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

                var md = await renderUseCase.ExecuteAsync(codeGraph, projectName, tplDir, rulesDir, agentType, endpoint, key, model);

                Console.WriteLine($"[docgen] Renderizado completo. Generando Markdown...");
                var outDir = Path.GetDirectoryName(outPath);
                if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);

                await File.WriteAllTextAsync(outPath, md);

                Console.WriteLine($"[docgen] Markdown generado exitosamente en: {outPath}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[error] Fallo en la renderización: {ex.Message}");
            }
        });

        return cmd;
    }
}