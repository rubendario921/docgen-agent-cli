namespace DocGen_Agent.Application.Ports;

public interface IValidator
{
    Task<(bool IsValid, string[] Errors)> ValidateAsync(string content);
}