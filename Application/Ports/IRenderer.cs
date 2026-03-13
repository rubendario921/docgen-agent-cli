namespace DocGen_Agent.Application.Ports;

public interface IRenderer
{
    string Render(string templateContent, object model);
}