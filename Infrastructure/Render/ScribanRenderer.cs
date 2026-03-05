using DocGen_Agent.Core.Abstractions;
using Scriban;

namespace DocGen_Agent.Infrastructure.Render;

public sealed class ScribanRenderer : IRenderer
{
    public string Render(string templateContent, object model)
    {
        var tpl = Template.Parse(templateContent);
        if (tpl.HasErrors)
        {
            var msg = string.Join(Environment.NewLine, tpl.Messages.Select(m => m.ToString()));
            throw new InvalidOperationException($"Error en plantilla: {msg}");
        }
        return tpl.Render(model, member => member.Name);
    }
}