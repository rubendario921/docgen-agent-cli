using DocGen_Agent.Application.Ports;
using DocGen_Agent.Domain.Entities;

namespace DocGen_Agent.Application.UseCases.ScanApplication;

public interface IScanUseCase
{
    CodeGraph Execute(string path);
}
