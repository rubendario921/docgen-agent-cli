namespace DocGen_Agent.Core.Models;

public sealed record CodeGraph(
    string Language,
    string Framework,
    IReadOnlyList<Component> Components,
    IReadOnlyList<Endpoint> Endpoints,
    IReadOnlyList<string> FilesScanned
);