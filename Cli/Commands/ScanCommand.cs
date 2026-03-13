using DocGen_Agent.Application.Ports;
using DocGen_Agent.Domain.Entities;
using DocGen_Agent.Infrastructure.Scanners;
using DocGen_Agent.Application.UseCases.ScanApplication;
using System.CommandLine;
using System.Text.Json;

namespace DocGen_Agent.Cli.Commands;

public static class ScanCommand
{
    public static Command Build(IScanUseCase scanUseCase)
    {
        var cmd = new Command("scan", "Escanea el repositorio y construye el grafo de código");

        var solution = new Option<string>("--solution", () => ".", "Ruta raíz del proyecto/solución");
        var outFile = new Option<string>("--out", description: "Archivo de salida (graph.json)") { IsRequired = true };

        cmd.AddOption(solution);
        cmd.AddOption(outFile);

        cmd.SetHandler((string solutionPath, string outPath) =>
        {
            try 
            {
                CodeGraph graph = scanUseCase.Execute(solutionPath);

                var outDir = Path.GetDirectoryName(outPath);
                if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);
                File.WriteAllText(outPath, JsonSerializer.Serialize(graph, new JsonSerializerOptions
                {
                    WriteIndented = true
                }));

                Console.WriteLine($"[docgen] Grafo generado: {outPath}");
            } 
            catch (Exception ex) 
            {
                Console.Error.WriteLine($"[error] {ex.Message}");
            }
        }, solution, outFile);

        return cmd;
    }
}