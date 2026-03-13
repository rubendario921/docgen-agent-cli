using DocGen_Agent.Application.Ports;
using System.Text.RegularExpressions;

namespace DocGen_Agent.Infrastructure.Validators;

public sealed class MermaidValidator : IValidator
{
    public Task<(bool IsValid, string[] Errors)> ValidateAsync(string content)
    {
        var errors = new List<string>();
        var mermaidBlocks = new Regex(@"```mermaid\s+(?<type>\w+)\s+(?<body>.*?)```", RegexOptions.Singleline);

        var matches = mermaidBlocks.Matches(content);
        foreach (Match match in matches)
        {
            var type = match.Groups["type"].Value;
            var body = match.Groups["body"].Value;

            if (type == "sequenceDiagram")
            {
                if (!body.Contains("participant") && !body.Contains("->"))
                {
                    errors.Add("Bloque Mermaid 'sequenceDiagram' parece estar vacío o mal formado.");
                }
            }

            // Validación básica de balanceo de paréntesis/corchetes en el cuerpo
            if (body.Count(f => f == '(') != body.Count(f => f == ')'))
                errors.Add("Paréntesis desbalanceados en bloque Mermaid.");
        }

        return Task.FromResult((errors.Count == 0, errors.ToArray()));
    }
}