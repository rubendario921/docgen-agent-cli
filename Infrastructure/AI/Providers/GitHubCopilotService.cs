using DocGen_Agent.Core.Constants;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace DocGen_Agent.Infrastructure.AI.Providers;

/// <summary>
/// Integracion con GitHub Copilot utilizando su endpoint compatible con OpenAI y adaptando a la interfaz común de ChatClient para mantener compatibilidad con el resto de la aplicación.
/// </summary>
public sealed class GitHubCopilotService : BaseAIService
{
    private readonly ChatClient _chatClient;

    public GitHubCopilotService(string endpoint, string key, string model)
    {
        endpoint = string.IsNullOrWhiteSpace(endpoint) ? AIConstants.DefaultCopilotEndpoint : endpoint;
        model = string.IsNullOrWhiteSpace(model) ? AIConstants.DefaultCopilotModel : model;
        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint)
        };

        var client = new OpenAIClient(new ApiKeyCredential(key), options);
        _chatClient = client.GetChatClient(model);
    }

    protected override bool UseSystemRole => true;

    protected override ChatClient GetChatClient() => _chatClient;
}