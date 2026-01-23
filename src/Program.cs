using System.Text;
using DiffLog.Commands;
using DiffLog.Infrastructure;
using DiffLog.Services;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;
using Spectre.Console;
using Spectre.Console.Cli;

Console.OutputEncoding = Encoding.UTF8;

if (args.Any(arg => arg is "-v" or "--version"))
{
    VersionPresenter.Show();
    return 0;
}

// Setup dependency injection
var services = new ServiceCollection();

// Register services
services.AddSingleton<IGitService, GitService>();
services.AddSingleton<IReleaseNoteGenerator, ReleaseNoteGenerator>();

// Configure AI Chat Client
// Uses OpenAI-compatible API - configure via environment variables or config:
// - OPENAI_API_KEY: Your API key
// - OPENAI_BASE_URL: Base URL for the API (optional, for Azure OpenAI or other providers)
// - OPENAI_MODEL: Model to use (defaults to gpt-4o)
// - difflog config: Persist these values to a config file
var savedConfig = AiConfigStore.Load();
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? savedConfig?.ApiKey;
var baseUrl = Environment.GetEnvironmentVariable("OPENAI_BASE_URL") ?? savedConfig?.BaseUrl;
var model = Environment.GetEnvironmentVariable("OPENAI_MODEL") ?? savedConfig?.Model ?? "gpt-4o";

if (string.IsNullOrEmpty(apiKey))
{
    AnsiConsole.MarkupLine("[yellow]Warning:[/] No OpenAI API key configured.");
    AnsiConsole.MarkupLine("[dim]Configure AI generation with environment variables:[/]");
    AnsiConsole.MarkupLine("[dim]  OPENAI_API_KEY - Your OpenAI API key[/]");
    AnsiConsole.MarkupLine("[dim]  OPENAI_BASE_URL - (Optional) Base URL for Azure OpenAI or compatible APIs[/]");
    AnsiConsole.MarkupLine("[dim]  OPENAI_MODEL - (Optional) Model to use (default: gpt-4o)[/]");
    AnsiConsole.MarkupLine("[dim]Or save them with: difflog config --api-key <KEY> --base-url <URL> --model <MODEL>[/]");
    AnsiConsole.WriteLine();
}

// Create OpenAI client
OpenAIClient? openAIClient = null;
if (!string.IsNullOrEmpty(apiKey))
{
    if (!string.IsNullOrEmpty(baseUrl))
    {
        openAIClient = new OpenAIClient(new System.ClientModel.ApiKeyCredential(apiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(baseUrl)
        });
    }
    else
    {
        openAIClient = new OpenAIClient(apiKey);
    }
}

// Register chat client
if (openAIClient != null)
{
    services.AddSingleton<IChatClient>(openAIClient.GetChatClient(model).AsIChatClient());
}
else
{
    // Register a placeholder that will show an error when used
    services.AddSingleton<IChatClient>(new PlaceholderChatClient());
}

// Show welcome when run without arguments
if (args.Length == 0)
{
    GenerateCommand.DisplayWelcome();
}

// Create the command app with DI
var registrar = new TypeRegistrar(services);
var app = new CommandApp(registrar);

app.Configure(config =>
{
    config.SetApplicationName("difflog");
    config.SetApplicationVersion("1.0.0");
    
    config.AddCommand<ConfigCommand>("config")
        .WithDescription("Store OpenAI configuration values for future runs.")
        .WithExample("config", "--api-key", "sk-...", "--model", "gpt-4o")
        .WithExample("config", "--interactive");

    config.AddCommand<GenerateCommand>("generate")
        .WithDescription("Generate release notes from git history.")
        .WithExample("generate")
        .WithExample("generate", "--from", "v1.0.0", "--to", "v2.0.0")
        .WithExample("generate", "-a", "EndUsers", "--format", "Html")
        .WithExample("generate", "-i");

    config.AddCommand<TagsCommand>("tags")
        .WithDescription("List available tags in the repository.");
});

return await app.RunAsync(args);

/// <summary>
/// Placeholder chat client that shows an error when AI is not configured.
/// </summary>
internal class PlaceholderChatClient : IChatClient
{
    public ChatOptions? DefaultOptions => null;
    public ChatClientMetadata Metadata => new("placeholder");

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "AI is not configured. Set OPENAI_API_KEY or run 'difflog config'.");
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "AI is not configured. Set OPENAI_API_KEY or run 'difflog config'.");
    }

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose() { }
}

