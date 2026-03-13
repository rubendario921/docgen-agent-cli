namespace DocGen_Agent.Domain.Entities;

public sealed record Component(
    string Kind,            // controller|service|repository|dto|module|usecase|interface
    string Name,
    string FilePath,
    IReadOnlyList<string> Deps
);