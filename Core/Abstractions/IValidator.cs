namespace DocGen_Agent.Core.Abstractions;

public interface IValidator
{
    Task<(bool IsValid, string[] Errors)> ValidateAsync(string content);
}