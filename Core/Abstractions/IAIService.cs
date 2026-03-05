using DocGen_Agent.Core.Models;

namespace DocGen_Agent.Core.Abstractions;

public interface IAIService
{
    Task<string> EnrichSectionAsync(string sectionName, CodeGraph graph, string projectName);

    Task<string> GenerateSequenceDiagramAsync(Endpoint endpoint, string projectContext);
}