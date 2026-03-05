using DocGen_Agent.Core.Abstractions;
using DocGen_Agent.Core.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DocGen_Agent.Infrastructure.Scanners;

/// <summary>
/// Scanner avanzado para .NET utilizando Microsoft.CodeAnalysis (Roslyn).
/// Proporciona una comprensión profunda de la estructura del código, atributos y tipos.
/// </summary>
public sealed class DotNetScanner : ISourceCodeScanner
{
    public CodeGraph BuildGraph(string solutionPath)
    {
        var files = Directory.GetFiles(solutionPath, "*.cs", SearchOption.AllDirectories);
        var endpoints = new List<Endpoint>();
        var components = new List<Component>();

        foreach (var path in files)
        {
            string text;
            try { text = File.ReadAllText(path); } catch { continue; }

            var tree = CSharpSyntaxTree.ParseText(text);
            var root = tree.GetCompilationUnitRoot();
            var relPath = ToRelative(path, solutionPath);

            // Analizar Clases
            var classes = root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            foreach (var cls in classes)
            {
                ProcessClass(cls, relPath, components, endpoints);
            }

            // Analizar Records (Comunes en DTOs de C# moderno)
            var records = root.DescendantNodes().OfType<RecordDeclarationSyntax>();
            foreach (var rec in records)
            {
                ProcessRecord(rec, relPath, components);
            }
        }

        return new CodeGraph(
            Language: "dotnet",
            Framework: "aspnet-core",
            Components: components,
            Endpoints: endpoints,
            FilesScanned: files.Select(p => ToRelative(p, solutionPath)).ToList()
        );
    }

    private void ProcessClass(ClassDeclarationSyntax cls, string relPath, List<Component> components, List<Endpoint> endpoints)
    {
        var className = cls.Identifier.Text;
        var attributes = cls.AttributeLists.SelectMany(al => al.Attributes).ToList();

        bool isController = attributes.Any(a => IsAttribute(a, "ApiController"));

        if (isController)
        {
            components.Add(new Component("controller", className, relPath, Array.Empty<string>()));

            // Obtener ruta base del controlador
            var routeAttr = attributes.FirstOrDefault(a => IsAttribute(a, "Route"));
            var baseRoute = GetAttributeArgument(routeAttr) ?? "";

            var methods = cls.Members.OfType<MethodDeclarationSyntax>();
            foreach (var method in methods)
            {
                var methodAttrs = method.AttributeLists.SelectMany(al => al.Attributes).ToList();
                var httpAttr = methodAttrs.FirstOrDefault(a => IsHttpAttribute(a));

                if (httpAttr != null)
                {
                    var verb = httpAttr.Name.ToString().Replace("Http", "").ToUpperInvariant();
                    if (string.IsNullOrEmpty(verb)) verb = "GET"; // Default si solo es [Route]

                    var methodRoute = GetAttributeArgument(httpAttr) ?? "";

                    // Limpieza y construcción de ruta completa
                    var fullPath = CombineRoutes(baseRoute, methodRoute, className);

                    endpoints.Add(new Endpoint(
                        Method: verb,
                        Path: fullPath,
                        Controller: className,
                        Handler: method.Identifier.Text,
                        RequestDto: null, // Roslyn permitiría extraer esto analizando parámetros
                        ResponseDto: null,
                        AuthRequired: methodAttrs.Any(a => IsAttribute(a, "Authorize")) || attributes.Any(a => IsAttribute(a, "Authorize")),
                        FilePath: relPath
                    ));
                }
            }
        }
        else if (className.EndsWith("Service") || attributes.Any(a => IsAttribute(a, "Service")))
        {
            components.Add(new Component("service", className, relPath, Array.Empty<string>()));
        }
        else if (className.EndsWith("UseCase"))
        {
            components.Add(new Component("usecase", className, relPath, Array.Empty<string>()));
        }
        else if (IsDto(className))
        {
            components.Add(new Component("dto", className, relPath, Array.Empty<string>()));
        }
    }

    private void ProcessRecord(RecordDeclarationSyntax rec, string relPath, List<Component> components)
    {
        var recordName = rec.Identifier.Text;
        if (IsDto(recordName))
        {
            components.Add(new Component("dto", recordName, relPath, Array.Empty<string>()));
        }
    }

    private bool IsAttribute(AttributeSyntax a, string name)
        => a.Name.ToString().Contains(name);

    private bool IsHttpAttribute(AttributeSyntax a)
    {
        var name = a.Name.ToString();
        return name.StartsWith("Http") || name.Equals("Route");
    }

    private string? GetAttributeArgument(AttributeSyntax? a)
    {
        if (a?.ArgumentList == null) return null;
        var arg = a.ArgumentList.Arguments.FirstOrDefault();
        return arg?.Expression.ToString().Trim('"');
    }

    private bool IsDto(string name)
        => name.EndsWith("Dto") || name.EndsWith("Request") || name.EndsWith("Response") || name.EndsWith("ViewModel");

    private string CombineRoutes(string baseRoute, string methodRoute, string controllerName)
    {
        var controllerToken = controllerName.Replace("Controller", "");
        var combined = $"{baseRoute}/{methodRoute}".Replace("//", "/").Replace("[controller]", controllerToken);
        if (!combined.StartsWith("/")) combined = "/" + combined;
        return combined.TrimEnd('/');
    }

    private static string ToRelative(string fullPath, string root)
        => Path.GetRelativePath(root, fullPath).Replace("\\", "/");
}