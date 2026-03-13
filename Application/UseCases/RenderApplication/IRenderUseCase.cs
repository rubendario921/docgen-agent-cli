using DocGen_Agent.Domain.Entities;

namespace DocGen_Agent.Application.UseCases.RenderApplication;

public interface IRenderUseCase
{
    Task<string> ExecuteAsync(CodeGraph codeGraph, string projectName, string templatesDir, string? rulesDir, string agentType, string aiEndpoint, string aiKey, string aiModel);
}
