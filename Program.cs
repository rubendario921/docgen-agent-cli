using DocGen_Agent.Cli.Commands;
using DocGen_Agent.Application.Ports;
using DocGen_Agent.Application.UseCases.ScanApplication;
using DocGen_Agent.Application.UseCases.RenderApplication;
using DocGen_Agent.Application.UseCases.VerifyApplication;
using DocGen_Agent.Application.UseCases.PublishApplication;
using DocGen_Agent.Infrastructure.Scanners;
using DocGen_Agent.Infrastructure.Render;
using DocGen_Agent.Infrastructure.AI;
using DocGen_Agent.Infrastructure.Publishers;
using DocGen_Agent.Infrastructure.Validators;
using System.CommandLine;

// 1. Setup Manual DI
var scanners = new List<ISourceCodeScanner> { new DotNetScanner(), new NodeScanner(), new AngularScanner() };
var scanUseCase = new ScanUseCase(scanners);

var aiFactory = new AIServiceFactory();
var renderer = new ScribanRenderer();
var renderUseCase = new RenderUseCase(renderer, aiFactory);

var validators = new List<IValidator> { new MermaidValidator(), new MarkdownLinkValidator() };
var verifyUseCase = new VerifyUseCase(validators);

var publishers = new List<IPublisher> { new AzureDevOpsWikiPublisher(new HttpClient()) };
var publishUseCase = new PublishUseCase(publishers);

// 2. Setup Commands
var root = new RootCommand("AzureDevOps.DocGen — CLI para generar documentación técnica");
root.AddCommand(ScanCommand.Build(scanUseCase));
root.AddCommand(RenderCommand.Build(renderUseCase));
root.AddCommand(VerifyCommand.Build(verifyUseCase));
root.AddCommand(PublishCommand.Build(publishUseCase));

return await root.InvokeAsync(args);