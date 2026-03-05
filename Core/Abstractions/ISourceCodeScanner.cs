using DocGen_Agent.Core.Models;

namespace DocGen_Agent.Core.Abstractions;

public interface ISourceCodeScanner
{
    CodeGraph BuildGraph(string solutionPath);
}