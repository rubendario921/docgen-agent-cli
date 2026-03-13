using DocGen_Agent.Domain.Entities;

namespace DocGen_Agent.Application.Ports;

public interface IAIService
{
    Task<string> EnrichSectionAsync(string sectionName, CodeGraph graph, string projectName);

    Task<string> GenerateSequenceDiagramAsync(Endpoint endpoint, string projectContext);
}