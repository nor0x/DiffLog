using System.ComponentModel;
using DiffLog.Models;
using Spectre.Console.Cli;

namespace DiffLog.Commands;

/// <summary>
/// Command settings for the generate command.
/// </summary>
public class GenerateSettings : CommandSettings
{
    [CommandOption("-p|--path <PATH>")]
    [Description("Path to the git repository. Defaults to current directory.")]
    [DefaultValue(".")]
    public string RepositoryPath { get; set; } = ".";

    [CommandOption("-f|--from <REF>")]
    [Description("Starting reference (tag, branch, or commit). If not specified, includes all commits up to --to.")]
    public string? FromRef { get; set; }

    [CommandOption("-t|--to <REF>")]
    [Description("Ending reference (tag, branch, or commit). Defaults to HEAD.")]
    [DefaultValue("HEAD")]
    public string ToRef { get; set; } = "HEAD";

    [CommandOption("-a|--audience <AUDIENCE>")]
    [Description("Target audience: Developers, EndUsers, SocialMedia, or Executive.")]
    [DefaultValue(Audience.Developers)]
    public Audience Audience { get; set; } = Audience.Developers;

    [CommandOption("--format <FORMAT>")]
    [Description("Output format: Markdown, Html, PlainText, or Json.")]
    [DefaultValue(OutputFormat.Markdown)]
    public OutputFormat Format { get; set; } = OutputFormat.Markdown;

    [CommandOption("-o|--output <FILE>")]
    [Description("Output file path. Defaults to a generated file in the repository.")]
    public string? OutputPath { get; set; }

    [CommandOption("--print")]
    [Description("Also print the generated release notes to the console.")]
    [DefaultValue(false)]
    public bool PrintToConsole { get; set; }

    [CommandOption("--from-date <DATE>")]
    [Description("Include commits from this date (yyyy-MM-dd).")]
    public string? FromDate { get; set; }

    [CommandOption("--to-date <DATE>")]
    [Description("Include commits until this date (yyyy-MM-dd).")]
    public string? ToDate { get; set; }

    [CommandOption("--no-links")]
    [Description("Exclude links to issues and PRs.")]
    [DefaultValue(false)]
    public bool NoLinks { get; set; }

    [CommandOption("--no-contributors")]
    [Description("Exclude contributor list.")]
    [DefaultValue(false)]
    public bool NoContributors { get; set; }

    [CommandOption("--repo-url <URL>")]
    [Description("Repository URL for generating links (auto-detected if not specified).")]
    public string? RepositoryUrl { get; set; }

    [CommandOption("--exclude <CATEGORIES>")]
    [Description("Comma-separated list of categories to exclude.")]
    public string? ExcludeCategories { get; set; }

    [CommandOption("-i|--interactive")]
    [Description("Run in interactive mode with prompts.")]
    [DefaultValue(false)]
    public bool Interactive { get; set; }

    [CommandOption("--system-prompt <PROMPT>")]
    [Description("Override the system prompt for this run.")]
    public string? SystemPrompt { get; set; }

    [CommandOption("--system-prompt-file <FILE>")]
    [Description("Path to a file containing a system prompt override.")]
    public string? SystemPromptFile { get; set; }
}
