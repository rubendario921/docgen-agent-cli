namespace DocGen_Agent.Core.Abstractions;

public interface IPublisher
{
    Task PublishAsync(string sourcePath, string destinationPath, object options);
}