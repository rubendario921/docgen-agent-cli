namespace DocGen_Agent.Application.Ports;

public interface IPublisher
{
    Task PublishAsync(string sourcePath, string destinationPath, object options);
}