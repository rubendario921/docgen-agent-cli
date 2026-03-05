using DocGen_Agent.Cli.Commands;
using System.CommandLine;

var root = new RootCommand("AzureDevOps.DocGen — CLI para generar documentación técnica");
root.AddCommand(ScanCommand.Build());
root.AddCommand(RenderCommand.Build());
root.AddCommand(VerifyCommand.Build());
root.AddCommand(PublishCommand.Build());

return await root.InvokeAsync(args);