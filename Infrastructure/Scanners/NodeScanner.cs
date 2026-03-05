using DocGen_Agent.Core.Abstractions;
using DocGen_Agent.Core.Models;
using System.Text.RegularExpressions;

namespace DocGen_Agent.Infrastructure.Scanners;

/// <summary>
/// Scanner para NodeJs(Typescript) nodos basado en heuristicas para atributos y regex.
/// </summary>
public sealed class NodeScanner : ISourceCodeScanner
{
    public CodeGraph BuildGraph(string solutionPath)
    {
        var files = Directory.GetFiles(solutionPath, "*.*", SearchOption.AllDirectories)
                             .Where(f => f.EndsWith(".ts", StringComparison.OrdinalIgnoreCase) ||
                                         f.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                             .ToArray();

        var endpoints = new List<Endpoint>();
        var components = new List<Component>();

        var nestCtrl = new Regex(@"@Controller\('?(?<base>[^')]+)'?\)\s*(export\s+)?class\s+(?<name>\w+)", RegexOptions.Singleline);
        var nestRoute = new Regex(@"@(Get|Post|Put|Delete)\('?(?<path>[^')]+)'?\)\s*(public|async)?\s*(?<handler>\w+)", RegexOptions.Singleline);
        var expRoute = new Regex(@"router\.(get|post|put|delete)\s*\(\s*['`](?<path>[^'`]+)['`]", RegexOptions.IgnoreCase);

        // Heurísticas Fase 3
        var injectable = new Regex(@"@Injectable\(\)\s*(export\s+)?class\s+(?<name>\w+)", RegexOptions.Singleline);
        var dtoPattern = new Regex(@"(interface|class)\s+(?<name>\w+(Dto|Request|Response|Entity))\b", RegexOptions.IgnoreCase);

        foreach (var path in files)
        {
            string text;
            try { text = File.ReadAllText(path); } catch { continue; }

            var rel = ToRelative(path, solutionPath);

            // NestJS Controller
            if (nestCtrl.IsMatch(text))
            {
                var c = nestCtrl.Match(text);
                var basePath = c.Groups["base"].Value;
                var name = c.Groups["name"].Value;
                components.Add(new Component("controller", name, rel, Array.Empty<string>()));

                foreach (Match m in nestRoute.Matches(text))
                {
                    var method = m.Groups[1].Value.ToUpperInvariant();
                    var relPath = m.Groups["path"].Value;
                    var handler = m.Groups["handler"].Value;
                    var full = $"/{basePath}/{relPath}".Replace("//", "/");
                    endpoints.Add(new Endpoint(method, full, name, handler, null, null, false, rel));
                }
            }
            // Injectable Service
            else if (injectable.IsMatch(text))
            {
                components.Add(new Component("service", injectable.Match(text).Groups["name"].Value, rel, Array.Empty<string>()));
            }
            // DTO / Entity
            else if (dtoPattern.IsMatch(text))
            {
                components.Add(new Component("dto", dtoPattern.Match(text).Groups["name"].Value, rel, Array.Empty<string>()));
            }
            // Express Router
            else if (expRoute.IsMatch(text))
            {
                components.Add(new Component("controller", "Router", rel, Array.Empty<string>()));
                foreach (Match m in expRoute.Matches(text))
                {
                    var method = m.Groups[1].Value.ToUpperInvariant();
                    var pathRel = m.Groups["path"].Value;
                    endpoints.Add(new Endpoint(method, pathRel, "Router", "handler", null, null, false, rel));
                }
            }
        }

        // Detección simple de framework
        var framework = DetectFramework(solutionPath);

        return new CodeGraph(
            Language: "node",
            Framework: framework,
            Components: components,
            Endpoints: endpoints,
            FilesScanned: files.Select(p => ToRelative(p, solutionPath)).ToList()
        );
    }

    private static string DetectFramework(string root)
    {
        var pkg = Directory.GetFiles(root, "package.json", SearchOption.AllDirectories).FirstOrDefault();
        if (pkg is null) return "node";
        var json = File.ReadAllText(pkg);
        if (json.Contains("\"@nestjs/core\"")) return "nestjs";
        if (json.Contains("\"express\"")) return "express";
        return "node";
    }

    private static string ToRelative(string fullPath, string root)
     => Path.GetRelativePath(root, fullPath).Replace("\\", "/");
}