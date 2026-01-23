using DiffLog.Infrastructure;
using DiffLog.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DiffLog.Commands;

public class ConfigCommand : AsyncCommand<ConfigSettings>
{
    public override Task<int> ExecuteAsync(CommandContext context, ConfigSettings settings, CancellationToken cancellationToken)
    {
        var existing = AiConfigStore.Load() ?? new AiConfiguration();

        if (!string.IsNullOrEmpty(settings.SystemPrompt) && !string.IsNullOrEmpty(settings.SystemPromptFile))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Provide either --system-prompt or --system-prompt-file, not both.");
            return Task.FromResult(1);
        }

        if ((settings.SystemPrompt != null || settings.SystemPromptFile != null) && settings.Audience == null)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] --audience is required to set a system prompt.");
            return Task.FromResult(1);
        }

        var updatingPrompt = settings.Audience != null
            && (settings.SystemPrompt != null || settings.SystemPromptFile != null || settings.Interactive);
        var apiKey = ResolveApiKey(settings, existing.ApiKey, updatingPrompt);
        if (apiKey == null && string.IsNullOrWhiteSpace(existing.ApiKey) && !updatingPrompt)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] API key is required. Use --api-key or --interactive.");
            return Task.FromResult(1);
        }

        var baseUrl = ResolveBaseUrl(settings, existing.BaseUrl);
        var model = ResolveModel(settings, existing.Model);

        var configuration = new AiConfiguration
        {
            ApiKey = apiKey ?? existing.ApiKey,
            BaseUrl = baseUrl,
            Model = model
        };

        configuration.AudienceSystemPrompts = existing.AudienceSystemPrompts;
        UpdateAudiencePrompt(configuration, settings);

        AiConfigStore.Save(configuration);

        AnsiConsole.MarkupLine("[green]✓[/] AI configuration saved.");
        AnsiConsole.MarkupLine($"[dim]Location:[/] {AiConfigStore.GetConfigPath()}");

        return Task.FromResult(0);
    }

    private static string? ResolveApiKey(ConfigSettings settings, string? existing, bool promptOnly)
    {
        if (promptOnly && string.IsNullOrWhiteSpace(settings.ApiKey) && !settings.Interactive)
        {
            return existing;
        }

        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return settings.ApiKey.Trim();
        }

        if (!settings.Interactive)
        {
            if (string.IsNullOrWhiteSpace(existing))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] API key is required. Use --api-key or --interactive.");
                return null;
            }

            return existing;
        }

        var prompt = new TextPrompt<string>("OpenAI API key:")
            .Secret()
            .AllowEmpty()
            .Validate(value => string.IsNullOrWhiteSpace(value) && string.IsNullOrWhiteSpace(existing)
                ? ValidationResult.Error("API key cannot be empty.")
                : ValidationResult.Success());

        var response = AnsiConsole.Prompt(prompt).Trim();
        return string.IsNullOrWhiteSpace(response) ? existing : response;
    }

    private static void UpdateAudiencePrompt(AiConfiguration configuration, ConfigSettings settings)
    {
        if (settings.Audience == null)
        {
            return;
        }

        var prompt = ResolveAudiencePrompt(settings);
        if (prompt == null)
        {
            return;
        }

        configuration.AudienceSystemPrompts[settings.Audience.Value] = prompt;
    }

    private static string? ResolveAudiencePrompt(ConfigSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.SystemPrompt))
        {
            return settings.SystemPrompt.Trim();
        }

        if (!string.IsNullOrWhiteSpace(settings.SystemPromptFile))
        {
            var path = settings.SystemPromptFile.Trim();
            if (!File.Exists(path))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] Prompt file not found: {path}");
                return null;
            }

            return File.ReadAllText(path).Trim();
        }

        if (!settings.Interactive)
        {
            return null;
        }

        var prompt = new TextPrompt<string>("System prompt:")
            .AllowEmpty()
            .Validate(value => string.IsNullOrWhiteSpace(value)
                ? ValidationResult.Error("System prompt cannot be empty.")
                : ValidationResult.Success());

        return AnsiConsole.Prompt(prompt).Trim();
    }

    private static string? ResolveBaseUrl(ConfigSettings settings, string? existing)
    {
        if (settings.BaseUrl != null)
        {
            var trimmed = settings.BaseUrl.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        if (!settings.Interactive)
        {
            return string.IsNullOrWhiteSpace(existing) ? null : existing;
        }

        var prompt = new TextPrompt<string>("Base URL (optional):")
            .AllowEmpty();

        if (!string.IsNullOrWhiteSpace(existing))
        {
            prompt.DefaultValue(existing);
        }

        var response = AnsiConsole.Prompt(prompt).Trim();
        return string.IsNullOrWhiteSpace(response) ? null : response;
    }

    private static string ResolveModel(ConfigSettings settings, string? existing)
    {
        if (!string.IsNullOrWhiteSpace(settings.Model))
        {
            return settings.Model.Trim();
        }

        if (!settings.Interactive)
        {
            return string.IsNullOrWhiteSpace(existing) ? "gpt-4o" : existing;
        }

        var defaultModel = string.IsNullOrWhiteSpace(existing) ? "gpt-4o" : existing;
        var prompt = new TextPrompt<string>("Model:")
            .DefaultValue(defaultModel)
            .Validate(value => string.IsNullOrWhiteSpace(value)
                ? ValidationResult.Error("Model cannot be empty.")
                : ValidationResult.Success());

        return AnsiConsole.Prompt(prompt).Trim();
    }
}
