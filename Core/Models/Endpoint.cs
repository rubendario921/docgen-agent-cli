namespace DocGen_Agent.Core.Models;

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