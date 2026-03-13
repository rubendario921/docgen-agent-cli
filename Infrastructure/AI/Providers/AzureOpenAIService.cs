using Azure;
using Azure.AI.OpenAI;
using DocGen_Agent.Domain.Constants;
using OpenAI.Chat;

namespace DocGen_Agent.Infrastructure.AI.Providers;

/// <summary>
/// Integracion con Azure OpenAI utilizando Azure.AI.OpenAI SDK y adaptando a la interfaz común de ChatClient para mantener compatibilidad con el resto de la aplicación.
/// </summary>
public sealed class AzureOpenAIService : BaseAIService
{
    private readonly ChatClient _chatClient;

    public AzureOpenAIService(string endpoint, string key, string model)
    {
        //Asignacion de valores por defecto
        endpoint = string.IsNullOrWhiteSpace(endpoint) ? AIConstants.DefaultOpenAIEndpoint : endpoint;
        model = string.IsNullOrWhiteSpace(model) ? AIConstants.DefaultOpenAIModel : model;

        var client = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
        _chatClient = client.GetChatClient(model);
    }

    protected override ChatClient GetChatClient() => _chatClient;
}