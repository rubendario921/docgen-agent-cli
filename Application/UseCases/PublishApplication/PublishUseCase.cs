using DocGen_Agent.Application.Ports;
using DocGen_Agent.Domain.Exceptions;

namespace DocGen_Agent.Application.UseCases.PublishApplication;

public class PublishUseCase : IPublishUseCase
{
    private readonly IEnumerable<IPublisher> _publishers;

    public PublishUseCase(IEnumerable<IPublisher> publishers)
    {
        _publishers = publishers;
    }

    public async Task ExecuteAsync(string sourcePath, string wikiPath, string orgUrl, string project, string wikiId, string accessToken)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new DomainException("No se encontró Token. Use --token o proporcione un SYSTEM_ACCESSTOKEN", "INVALID_TOKEN");

        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new DomainException("La ruta de origen no puede estar vacía", "INVALID_DOCUMENT_PATH");

        if (!File.Exists(sourcePath))
            throw new DomainException($"El archivo no existe: {sourcePath}", "DOCUMENT_NOT_FOUND");

        // In a real scenario we could select a specific publisher based on a config or parameter. 
        // For now, defaulting to AzureDevOpsPublisher.
        var publisher = _publishers.FirstOrDefault(p => p.GetType().Name == "AzureDevOpsWikiPublisher") 
            ?? throw new DomainException("AzureDevOpsWikiPublisher no registrado en el sistema");

        var options = new WikiPublishOptions(orgUrl, project, wikiId, accessToken);
        
        await publisher.PublishAsync(sourcePath, wikiPath, options);
    }
}
