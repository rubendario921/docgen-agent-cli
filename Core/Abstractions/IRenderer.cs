namespace DocGen_Agent.Core.Abstractions;

public interface IRenderer
{
    string Render(string templateContent, object model);
}