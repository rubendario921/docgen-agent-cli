using System.CommandLine;
using DocGen_Agent.Infrastructure.Publishers;

namespace DocGen_Agent.Cli.Commands;

public static class PublishCommand
{
    public static Command Build()
    {
        var cmd = new Command("publish", "Publica la documentación en Azure DevOps Wiki");

        var source = new Option<string>("--source") { IsRequired = true, Description = "Ruta al archivo .md generado" };
        var wikiPath = new Option<string>("--wiki-path") { IsRequired = true, Description = "Ruta destino en la Wiki (ej. /Arquitectura)" };
        var orgUrl = new Option<string>("--org-url") { IsRequired = true };
        var project = new Option<string>("--project") { IsRequired = true };
        var wikiId = new Option<string>("--wiki-id") { IsRequired = true };
        var token = new Option<string>("--token") { IsRequired = false, Description = "System.AccessToken o PAT" };

        cmd.AddOption(source);
        cmd.AddOption(wikiPath);
        cmd.AddOption(orgUrl);
        cmd.AddOption(project);
        cmd.AddOption(wikiId);
        cmd.AddOption(token);

        cmd.SetHandler(async (sourcePath, path, url, proj, id, tkn) =>
        {
            try
            {
                var accessToken = tkn ?? Environment.GetEnvironmentVariable("SYSTEM_ACCESSTOKEN");
                if (string.IsNullOrEmpty(accessToken))
                {
                    throw new Exception("[error] No se encontró Token. Use --token o SYSTEM_ACCESSTOKEN");
                }

                Console.WriteLine($"[docgen] Publicando {sourcePath} en Wiki {id}...");

                using var httpClient = new HttpClient();
                var publisher = new AzureDevOpsWikiPublisher(httpClient);
                var options = new WikiPublishOptions(url, proj, id, accessToken);

                await publisher.PublishAsync(sourcePath, path, options);

                Console.WriteLine("[docgen] Publicación exitosa.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"[critical] Error en publicación: {ex.Message}");
            }
        }, source, wikiPath, orgUrl, project, wikiId, token);

        return cmd;
    }
}