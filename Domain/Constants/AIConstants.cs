namespace DocGen_Agent.Domain.Constants;

public static class AIConstants
{
    // Azure OpenAI
    public const string DefaultOpenAIEndpoint = "https://api.openai.com/v1";

    public const string DefaultOpenAIModel = "gpt-4o";

    // GitHub Models / Copilot
    public const string DefaultCopilotEndpoint = "https://models.inference.ai.azure.com";

    public const string DefaultCopilotModel = "gpt-4o";

    // Gemini (via Google AI Studio OpenAI-Compatible endpoint)
    public const string DefaultGeminiEndpoint = "https://generativelanguage.googleapis.com";

    public const string DefaultGeminiModel = "gemini-2.5-flash";
}