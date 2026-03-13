using DocGen_Agent.Application.Ports;
using DocGen_Agent.Domain.Exceptions;
using DocGen_Agent.Infrastructure.AI.Providers;

namespace DocGen_Agent.Infrastructure.AI;

public class AIServiceFactory : IAIServiceFactory
{
    public IAIService Create(string agentType, string endpoint, string key, string model)
    {
        return agentType.ToLower() switch
        {
            "azure" => new AzureOpenAIService(endpoint, key, model),
            "copilot" => new GitHubCopilotService(endpoint, key, model),
            "gemini" => new GeminiService(endpoint, key, model),
            _ => throw new DomainException($"Tipo de agente '{agentType}' no válido")
        };
    }
}
