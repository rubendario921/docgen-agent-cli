using DocGen_Agent.Application.Ports;
using DocGen_Agent.Domain.Entities;
using DocGen_Agent.Infrastructure.AI.Prompts;
using OpenAI.Chat;
using System.Text.Json;

namespace DocGen_Agent.Infrastructure.AI.Providers;

public abstract class BaseAIService : IAIService
{
    protected abstract ChatClient GetChatClient();

    public async Task<string> EnrichSectionAsync(string sectionName, CodeGraph graph, string projectName)
    {
        string prompt = sectionName switch
        {
            "ExecutiveSummary" => string.Format(PromptTemplates.ExecutiveSummaryPrompt, projectName, JsonSerializer.Serialize(graph)),
            _ => throw new ArgumentException($"Sección {sectionName} no soportada.")
        };

        return await GetChatCompletionAsync(prompt);
    }

    public async Task<string> GenerateSequenceDiagramAsync(Endpoint endpoint, string projectContext)
    {
        string prompt = string.Format(PromptTemplates.SequenceDiagramPrompt,
            endpoint, projectContext);

        return await GetChatCompletionAsync(prompt);
    }

    protected virtual bool UseSystemRole => true;

    private async Task<string> GetChatCompletionAsync(string prompt)
    {
        var messages = new List<ChatMessage>();

        if (UseSystemRole)
        {
            messages.Add(new SystemChatMessage(PromptTemplates.SystemRole));
            messages.Add(new UserChatMessage(prompt));
        }
        else
        {
            messages.Add(new UserChatMessage($"{PromptTemplates.SystemRole}\n\nContexto:\n{prompt}"));
        }

        var client = GetChatClient();
        try
        {
            ChatCompletion completion = await client.CompleteChatAsync(messages);
            return completion.Content[0].Text?.Trim() ?? "_(Error al generar contenido con IA)_";
        }
        catch (System.ClientModel.ClientResultException ex)
        {
            throw new Exception($"IA Provider Error: {ex.Status} {ex.Message}");
        }
    }
}