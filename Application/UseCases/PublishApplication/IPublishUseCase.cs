namespace DocGen_Agent.Application.UseCases.PublishApplication;

public interface IPublishUseCase
{
    Task ExecuteAsync(string sourcePath, string wikiPath, string orgUrl, string project, string wikiId, string accessToken);
}
