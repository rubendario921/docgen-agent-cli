using DocGen_Agent.Domain.Entities;

namespace DocGen_Agent.Application.Ports;

public interface ISourceCodeScanner
{
    CodeGraph BuildGraph(string solutionPath);
}