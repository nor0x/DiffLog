using System.ComponentModel;
using DiffLog.Models;
using Spectre.Console.Cli;

namespace DiffLog.Commands;

public class ConfigSettings : CommandSettings
{
    [CommandOption("--api-key <KEY>")]
    [Description("OpenAI API key to store.")]
    public string? ApiKey { get; set; }

    [CommandOption("--base-url <URL>")]
    [Description("Base URL for OpenAI-compatible API.")]
    public string? BaseUrl { get; set; }

    [CommandOption("--model <MODEL>")]
    [Description("Model to use for generation.")]
    public string? Model { get; set; }

    [CommandOption("--audience <AUDIENCE>")]
    [Description("Audience to set a custom system prompt for.")]
    public Audience? Audience { get; set; }

    [CommandOption("--system-prompt <PROMPT>")]
    [Description("Custom system prompt to store for the audience.")]
    public string? SystemPrompt { get; set; }

    [CommandOption("--system-prompt-file <FILE>")]
    [Description("Path to a file containing the system prompt.")]
    public string? SystemPromptFile { get; set; }

    [CommandOption("-i|--interactive")]
    [Description("Prompt for missing values.")]
    [DefaultValue(false)]
    public bool Interactive { get; set; }
}
