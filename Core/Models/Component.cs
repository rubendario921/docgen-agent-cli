namespace DocGen_Agent.Core.Models;

public sealed record Component(
    string Kind,            // controller|service|repository|dto|module|usecase|interface
    string Name,
    string FilePath,
    IReadOnlyList<string> Deps
);