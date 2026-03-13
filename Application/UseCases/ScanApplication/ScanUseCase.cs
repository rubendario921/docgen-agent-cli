using DocGen_Agent.Application.Ports;
using DocGen_Agent.Domain.Entities;
using DocGen_Agent.Domain.Exceptions;

namespace DocGen_Agent.Application.UseCases.ScanApplication;

public class ScanUseCase : IScanUseCase
{
    private readonly IEnumerable<ISourceCodeScanner> _scanners;

    public ScanUseCase(IEnumerable<ISourceCodeScanner> scanners)
    {
        _scanners = scanners;
    }

    public CodeGraph Execute(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new DomainException("El path no puede ser vacío.", "INVALID_PATH");

        ISourceCodeScanner scanner = DetectScanner(path);
        return scanner.BuildGraph(path);
    }

    private ISourceCodeScanner DetectScanner(string root)
    {
        //.Net
        var hasCsproj = Directory.EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories).Any();
        if (hasCsproj) return _scanners.FirstOrDefault(s => s.GetType().Name == "DotNetScanner") 
            ?? throw new DomainException("DotNetScanner no registrado");

        //Angular
        var angularJson = Path.Combine(root, "angular.json");
        var anyPkg = Directory.EnumerateFiles(root, "package.json", SearchOption.AllDirectories).FirstOrDefault(p => !p.Replace("\\", "/").Contains("/node_modules/"));
        bool isAngular = File.Exists(angularJson) || (anyPkg != null && File.ReadAllText(anyPkg).Contains("\"@angular/core\""));
        if (isAngular) return _scanners.FirstOrDefault(s => s.GetType().Name == "AngularScanner")
            ?? throw new DomainException("AngularScanner no registrado");

        //Node(Nest/Expresss)
        var hasPkg = Directory.EnumerateFiles(root, "package.json", SearchOption.AllDirectories).Any();
        if (hasPkg) return _scanners.FirstOrDefault(s => s.GetType().Name == "NodeScanner")
            ?? throw new DomainException("NodeScanner no registrado");

        throw new DomainException("No se detectó stack .NET, Node, Angular", "UNSUPPORTED_FRAMEWORK");
    }
}
