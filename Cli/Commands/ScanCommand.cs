using DocGen_Agent.Core.Abstractions;
using DocGen_Agent.Core.Models;
using DocGen_Agent.Infrastructure.Scanners;
using System.CommandLine;
using System.Text.Json;

namespace DocGen_Agent.Cli.Commands;

public static class ScanCommand
{
    public static Command Build()
    {
        var cmd = new Command("scan", "Escanea el repositorio y construye el grafo de código");

        var solution = new Option<string>("--solution", () => ".", "Ruta raíz del proyecto/solución");
        var outFile = new Option<string>("--out", description: "Archivo de salida (graph.json)") { IsRequired = true };

        cmd.AddOption(solution);
        cmd.AddOption(outFile);

        cmd.SetHandler((string solutionPath, string outPath) =>
        {
            ISourceCodeScanner scanner = DetectScanner(solutionPath);
            CodeGraph graph = scanner.BuildGraph(solutionPath);

            var outDir = Path.GetDirectoryName(outPath);
            if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);
            File.WriteAllText(outPath, JsonSerializer.Serialize(graph, new JsonSerializerOptions
            {
                WriteIndented = true
            }));

            Console.WriteLine($"[docgen] Grafo generado: {outPath}");
        }, solution, outFile);

        return cmd;
    }

    private static ISourceCodeScanner DetectScanner(string root)
    {
        //.Net
        var hasCsproj = Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories).Any();
        if (hasCsproj) return new DotNetScanner();

        //Angular
        var angularJson = Path.Combine(root, "angular.json");
        var anyPkg = Directory.EnumerateFiles(root, "package.json", SearchOption.AllDirectories).FirstOrDefault(p => !p.Replace("\\", "/").Contains("/node_modules/"));
        bool isAngular = File.Exists(angularJson) || (anyPkg != null && File.ReadAllText(anyPkg).Contains("\"@angular/core\""));
        if (isAngular) return new AngularScanner();

        //Node(Nest/Expresss)
        var hasPkg = Directory.EnumerateFiles(root, "package.json", SearchOption.AllDirectories).Any();
        if (hasPkg) return new NodeScanner();

        throw new InvalidOperationException("No se detectó stack .NET, Node, Angular");
    }
}