using DocGen_Agent.Application.Ports;
using DocGen_Agent.Domain.Exceptions;

namespace DocGen_Agent.Application.UseCases.VerifyApplication;

public class VerifyUseCase : IVerifyUseCase
{
    private readonly IEnumerable<IValidator> _validators;

    public VerifyUseCase(IEnumerable<IValidator> validators)
    {
        _validators = validators;
    }

    public async Task ExecuteAsync(string docsPath)
    {
        if (string.IsNullOrWhiteSpace(docsPath))
            throw new DomainException("La ruta de documentación no puede estar vacía", "INVALID_PATH");

        if (!Directory.Exists(docsPath) && !File.Exists(docsPath))
            throw new DomainException($"No se encontró el directorio o archivo: {docsPath}", "PATH_NOT_FOUND");

        var mdFiles = File.Exists(docsPath) && docsPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                        ? new[] { docsPath }
                        : Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories);

        if (mdFiles.Length == 0)
            throw new DomainException("No se encontraron archivos Markdown (.md) para validar.", "NO_MARKDOWN_FILES");

        bool anyError = false;

        foreach (var file in mdFiles)
        {
            var content = await File.ReadAllTextAsync(file);
            foreach (var validator in _validators)
            {
                var (isValid, errors) = await validator.ValidateAsync(content);
                if (!isValid)
                {
                    anyError = true;
                    foreach (var err in errors)
                    {
                        Console.WriteLine($"[warn] {file} - {validator.GetType().Name}: {err}");
                    }
                }
            }
        }

        if (anyError)
        {
            throw new DomainException("Se encontraron errores de validación. Revise los warnings.", "VALIDATION_ERRORS");
        }

        return;
    }
}
