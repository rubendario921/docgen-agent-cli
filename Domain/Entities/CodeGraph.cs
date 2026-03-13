namespace DocGen_Agent.Domain.Entities;

public sealed record CodeGraph(
    string Language,
    string Framework,
    IReadOnlyList<Component> Components,
    IReadOnlyList<Endpoint> Endpoints,
    IReadOnlyList<string> FilesScanned
);