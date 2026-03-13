namespace DocGen_Agent.Application.Ports;

public record WikiPublishOptions(string OrgUrl, string Project, string WikiId, string Token, string? Branch = null, string? Comment = null);
