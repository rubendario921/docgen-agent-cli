namespace DocGen_Agent.Domain.Entities;

public sealed record Endpoint(
    string Method,
    string Path,
    string Controller,
    string Handler,
    string? RequestDto,
    string? ResponseDto,
    bool AuthRequired,
    string FilePath
);