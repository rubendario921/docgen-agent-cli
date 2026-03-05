using DocGen_Agent.Core.Abstractions;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DocGen_Agent.Infrastructure.Publishers;

public sealed class AzureDevOpsWikiPublisher : IPublisher
{
    private readonly HttpClient _httpClient;

    public AzureDevOpsWikiPublisher(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task PublishAsync(string sourcePath, string destinationPath, object options)
    {
        if (options is not WikiPublishOptions wikiOptions) throw new ArgumentException("Se requieren WikiPublishOptions", nameof(options));

        //Contenido a Publicar
        var content = await File.ReadAllTextAsync(sourcePath);
        var body = JsonSerializer.Serialize(new { content });

        //Aut
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{wikiOptions.Token}")));

        //Descubrir Wiki
        var wikiInfoUrl = $"{wikiOptions.OrgUrl}/{wikiOptions.Project}/_apis/wiki/wikis/{Uri.EscapeDataString(wikiOptions.WikiId)}?api-version=7.1";
        var wikiInfo = await _httpClient.GetFromJsonAsync<WikiInfo>(wikiInfoUrl) ?? throw new Exception($"No se pudo obtener información de la Wiki con ID '{wikiOptions.WikiId}'");

        var isCodeWiki = string.Equals(wikiInfo.type, "codewiki", StringComparison.OrdinalIgnoreCase);
        var defaultBranch = isCodeWiki ? wikiInfo.versions?.FirstOrDefault()?.version : null; // Ex: main o master

        //URL de publicacion
        var encodedPath = Uri.EscapeDataString(destinationPath);
        var baseUrl = $"{wikiOptions.OrgUrl}/{wikiOptions.Project}/_apis/wiki/wikis/{Uri.EscapeDataString(wikiOptions.WikiId)}/pages?path={encodedPath}";

        var qb = new StringBuilder();
        if (isCodeWiki)
        {
            var branch = string.IsNullOrWhiteSpace(wikiOptions.Branch) ? defaultBranch : wikiOptions.Branch;
            if (string.IsNullOrEmpty(branch)) throw new InvalidOperationException("Code Wiki requiere rama  (versionDescriptor.version)");

            qb.Append($"&versionDescriptor.versionType=branch&versionDescriptor.version={Uri.EscapeDataString(branch)}");
        }

        if (!string.IsNullOrWhiteSpace(wikiOptions.Comment)) qb.Append($"&comment={Uri.EscapeDataString(wikiOptions.Comment)}");

        qb.Append("&api-version=7.1");
        var requestUrl = qb.ToString();
        Console.WriteLine($"[docgen] URL de petición: {wikiOptions.OrgUrl}/{wikiOptions.Project}/_apis/wiki/wikis/{wikiOptions.WikiId}/pages (Path: {destinationPath})");

        //Creacion del Wiki
        using var req = new HttpRequestMessage(HttpMethod.Put, requestUrl)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        var res = await _httpClient.SendAsync(req);
        if (!res.IsSuccessStatusCode)
        {
            var error = await res.Content.ReadAsStringAsync();
            throw new Exception($"Error al publicar en Wiki: {res.StatusCode} - {error}");
        }
    }

    private sealed record WikiInfo(string id, string type, List<WikiVersion>? versions);
    private sealed record WikiVersion(string version);
}

public record WikiPublishOptions(string OrgUrl, string Project, string WikiId, string Token, string? Branch = null, string? Comment = null);