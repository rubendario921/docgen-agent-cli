namespace DocGen_Agent.Application.UseCases.VerifyApplication;

public interface IVerifyUseCase
{
    Task ExecuteAsync(string docsPath);
}
