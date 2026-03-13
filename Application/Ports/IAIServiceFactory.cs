namespace DocGen_Agent.Application.Ports;

public interface IAIServiceFactory
{
    IAIService Create(string agentType, string endpoint, string key, string model);
}
