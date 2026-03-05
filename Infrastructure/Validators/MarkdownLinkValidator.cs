using DocGen_Agent.Core.Abstractions;
using System.Text.RegularExpressions;

namespace DocGen_Agent.Infrastructure.Validators;

public sealed class MarkdownLinkValidator : IValidator
{
    public Task<(bool IsValid, string[] Errors)> ValidateAsync(string content)
    {
        var errors = new List<string>();
        // Regex para buscar links externos (http/https) que deberían ser relativos según el Plan Maestro
        var externalLinkRegex = new Regex(@"\[([^\]]+)\]\((https?://[^\)]+)\)");

        var matches = externalLinkRegex.Matches(content);
        foreach (Match match in matches)
        {
            var url = match.Groups[2].Value;
            // Permitimos links externos si son explícitamente necesarios, pero avisamos.
            // Una regla de Clean Docs es preferir rutas relativas para archivos del repo.
            if (url.Contains("github.com") || url.Contains("visualstudio.com"))
            {
                // Podría ser un link al repo, pero el Plan Maestro dice "enlazar archivos con ruta relativa"
                errors.Add($"Link externo detectado: {url}. Considere usar rutas relativas para archivos del repositorio.");
            }
        }

        return Task.FromResult((errors.Count == 0, errors.ToArray()));
    }
}