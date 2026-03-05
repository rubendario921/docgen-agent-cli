using DocGen_Agent.Core.Constants;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace DocGen_Agent.Infrastructure.AI.Providers;

/// <summary>
/// Integración con Google Gemini utilizando su endpoint compatible con OpenAI.
/// </summary>
public sealed class GeminiService : BaseAIService
{
    private readonly ChatClient _chatClient;

    public GeminiService(string endpoint, string Key, string model)
    {
        //Asignacion de valores por defecto
        endpoint = string.IsNullOrWhiteSpace(endpoint) ? AIConstants.DefaultGeminiEndpoint : endpoint;
        model = string.IsNullOrWhiteSpace(model) ? AIConstants.DefaultGeminiModel : model;

        var options = new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint)
        };

        var client = new OpenAIClient(new ApiKeyCredential(Key), options);
        _chatClient = client.GetChatClient(model);
    }

    protected override bool UseSystemRole => true;

    protected override ChatClient GetChatClient() => _chatClient;
}