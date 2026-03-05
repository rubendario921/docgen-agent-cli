using System.CommandLine;
using DocGen_Agent.Core.Abstractions;
using DocGen_Agent.Infrastructure.Validators;

namespace DocGen_Agent.Cli.Commands;

public static class VerifyCommand
{
    public static Command Build()
    {
        var cmd = new Command("verify", "Valida la calidad del Markdown generado (Mermaid, Links, etc.)");

        var file = new Option<string>("--file") { IsRequired = true };

        cmd.AddOption(file);

        cmd.SetHandler(async (string filePath) =>
        {
            try 
            {
                if (!File.Exists(filePath))
                {
                    Console.Error.WriteLine($"[error] No se encontró el archivo: {filePath}");
                    return;
                }

                var content = await File.ReadAllTextAsync(filePath);
                var validators = new List<IValidator>
                {
                    new MermaidValidator(),
                    new MarkdownLinkValidator()
                };

                bool allValid = true;
                foreach (var v in validators)
                {
                    var (isValid, errors) = await v.ValidateAsync(content);
                    if (!isValid)
                    {
                        allValid = false;
                        foreach (var err in errors)
                        {
                            Console.WriteLine($"[warn] {v.GetType().Name}: {err}");
                        }
                    }
                }

                if (allValid)
                    Console.WriteLine("[docgen] Verificación exitosa. El documento cumple los estándares.");
                else
                    Console.WriteLine("[docgen] Verificación completada con advertencias.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[critical] Error en verificación: {ex.Message}");
            }
        }, file);

        return cmd;
    }
}
