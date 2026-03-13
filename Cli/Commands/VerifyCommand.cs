using System.CommandLine;
using DocGen_Agent.Application.Ports;
using DocGen_Agent.Application.UseCases.VerifyApplication;
using DocGen_Agent.Infrastructure.Validators;

namespace DocGen_Agent.Cli.Commands;

public static class VerifyCommand
{
    public static Command Build(IVerifyUseCase verifyUseCase)
    {
        var cmd = new Command("verify", "Valida la calidad del Markdown generado (Mermaid, Links, etc.)");

        var file = new Option<string>("--file") { IsRequired = true };

        cmd.AddOption(file);

        cmd.SetHandler(async (string filePath) =>
        {
            try 
            {
                await verifyUseCase.ExecuteAsync(filePath);
                Console.WriteLine("[docgen] Verificación exitosa. El documento cumple los estándares.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[critical] Error en verificación: {ex.Message}");
            }
        }, file);

        return cmd;
    }
}
