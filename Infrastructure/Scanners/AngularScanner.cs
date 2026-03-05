using DocGen_Agent.Core.Abstractions;
using DocGen_Agent.Core.Models;
using System.Text.RegularExpressions;

namespace DocGen_Agent.Infrastructure.Scanners;

/// <summary>
/// Scanner para Angular basado en heuristicas para atributos y regex.
/// Detecta componentes, servicios, rutas, modelos módulos y llamadas HttpClient.
/// </summary>
public sealed class AngularScanner : ISourceCodeScanner
{
    public CodeGraph BuildGraph(string solutionPath)
    {
        //Deteccion de workspace Angular y Framework
        var framework = DetectAngularFramework(solutionPath);

        //Obtener archivos TS relevantes (excluye *.spec.ts y node_modules)
        var files = Directory.GetFiles(solutionPath, "*.ts", SearchOption.AllDirectories)
                             .Where(f => !f.Replace("\\", "/").Contains("/node_modules/") &&
                                         !f.EndsWith(".spec.ts"))
                             .ToList();
        var components = new List<Component>();
        var endpoints = new List<Endpoint>();

        //RegEx Basicas (Toleraciones a espacios y nuevas lineas)
        var rxNgModule = new Regex(@"@\s*NgModule\s*\(\s*\{[\s\S]*?\}\s*\)\s*export\s+class\s+(?<name>\w+)", RegexOptions.Compiled);
        var rxComponent = new Regex(@"@\s*Component\s*\(\s*\{?<selector>[^'`]+['`][\s\S]*?\}\s*\)\s*export\s+class\s+(?<name>\w+)", RegexOptions.Compiled);
        var rxInjectable = new Regex(@"@\s*Injectable\s*\(\s*\{?[\s\S]*?\}?\s*\)\s*export\s+class\s+(?<name>\w+)", RegexOptions.Compiled);
        var rxRouterBlock = new Regex(@"RouterModule\.for(Root|Child)\s*\(\s*\[(?<routes>[\s\S]*?)\]\s*\)", RegexOptions.Compiled);
        var rxRouteItem = new Regex(@"\{\s*path\s*:\s*?<path>[^'`]+['`][\s\S]*?(?:component\s*:\s*(?<component>\w+))?[\s\S]*?(?:loadChildren\s*:\s*(?<loadChildren>[^,}]+))?[\s\S]*?\}", RegexOptions.Compiled);
        //HttpClient llamadas(string literal)
        var rxHttpCall = new Regex(@"(?<method>get|post|put|delete|patch)\s*<?<url>[^'`]+['`]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var rxHttpCall2 = new Regex(@"(?<method>get|post|put|delete|patch)\s*\(\s*?<url>[^'`]+['`]", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        var rxClassName = new Regex(@"export\s+class\s+(?<name>\w+)", RegexOptions.Compiled);
        var rxMethodScope = new Regex(@"(?<name>\w+)\s*\([^)]*\)\s*{[\s\S]*?$", RegexOptions.Compiled);

        foreach (var f in files)
        {
            string text;
            try { text = File.ReadAllText(f); } catch { continue; }
            var rel = ToRel(f, solutionPath);

            //NgModule
            foreach (Match m in rxNgModule.Matches(text))
            {
                var name = m.Groups["name"].Value;
                components.Add(new Component("ng-module", name, rel, Array.Empty<string>()));
            }

            //Component
            foreach (Match m in rxComponent.Matches(text))
            {
                var name = m.Groups["name"].Value;
                components.Add(new Component("ng-component", name, rel, Array.Empty<string>()));
            }

            //Service
            foreach (Match m in rxInjectable.Matches(text))
            {
                var name = m.Groups["name"].Value;
                components.Add(new Component("ng-service", name, rel, Array.Empty<string>()));

                //Búsqueda de llamadas HttpClient en el archivo del servicio
                //Intentamos obtener el nombre de la clase para asociar las llamadas
                var className = rxClassName.Match(text) is Match cm && cm.Success ? cm.Groups["name"].Value : name;

                foreach (Match h in rxHttpCall.Matches(text).Cast<Match>().Concat(rxHttpCall2.Matches(text).Cast<Match>()))
                {
                    var method = "HTTP." + h.Groups["method"].Value.ToUpperInvariant();
                    var url = h.Groups["url"].Value;

                    //Intento simple de aproximar el método TS envolvente (handler)
                    var handler = GuessEnclosingMethodName(text, h.Index) ?? "call";

                    endpoints.Add(new Endpoint(
                        Method: method,
                        Path: url,
                        Controller: className,
                        Handler: handler,
                        RequestDto: null,
                        ResponseDto: null,
                        AuthRequired: false,
                        FilePath: rel
                    ));
                }
            }

            //Routes (forRoot / forChild)
            foreach (Match b in rxRouterBlock.Matches(text))
            {
                var routesBlock = b.Groups["routes"].Value;
                foreach (Match r in rxRouteItem.Matches(routesBlock))
                {
                    var path = "/" + r.Groups["path"].Value.Trim('/');
                    var comp = r.Groups["component"].Success ? r.Groups["component"].Value : (r.Groups["loadChildren"].Success ? "LazyModule" : "UnknownComponent");

                    //Registramos la 'ruta' como un componente lógico
                    components.Add(new Component("ng-route", $"{path}", rel, Array.Empty<string>()));

                    endpoints.Add(new Endpoint(
                        Method: "ROUTE",
                        Path: path,
                        Controller: comp,
                        Handler: "render",
                        RequestDto: null,
                        ResponseDto: null,
                        AuthRequired: false,
                        FilePath: rel
                    ));
                }
            }
        }

        return new CodeGraph(
            Language: "typescript",
            Framework: "angular",
            Components: components,
            Endpoints: endpoints,
            FilesScanned: files.Select(f => ToRel(f, solutionPath)).ToList()
        );
    }

    private static string DetectAngularFramework(string root)
    {
        var hasAngularJson = File.Exists(Path.Combine(root, "angular.json"));
        if (hasAngularJson) return "angular";

        // fallback: leer package.json
        var pkg = Directory.GetFiles(root, "package.json", SearchOption.AllDirectories)
                           .FirstOrDefault(p => !p.Replace("\\", "/").Contains("/node_modules/"));
        if (pkg is not null)
        {
            var content = File.ReadAllText(pkg);
            if (content.Contains("\"@angular/core\"")) return "angular";
        }
        return "angular";
    }

    private static string ToRel(string path, string root)
     => Path.GetRelativePath(root, path).Replace("\\", "/");

    private static string? GuessEnclosingMethodName(string fileContent, int index)
    {
        // Busca hacia atrás la firma de método más cercana "<name>("
        const int lookback = 2000;
        var start = Math.Max(0, index - lookback);
        var slice = fileContent.Substring(start, index - start);
        // patrón para "methodName(args) {"
        var rx = new Regex(@"(?<name>\w+)\s*\([^)]*\)\s*\{", RegexOptions.RightToLeft);
        var m = rx.Match(slice);
        if (m.Success) return m.Groups["name"].Value;
        return null;
    }
}