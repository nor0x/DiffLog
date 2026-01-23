namespace DiffLog.Models;

/// <summary>
/// Options for generating release notes.
/// </summary>
public record ReleaseNoteOptions
{
    /// <summary>
    /// The target audience for the release notes.
    /// </summary>
    public Audience Audience { get; init; } = Audience.Developers;

    /// <summary>
    /// The output format for the release notes.
    /// </summary>
    public OutputFormat Format { get; init; } = OutputFormat.Markdown;

    /// <summary>
    /// The starting reference (tag, branch, or commit hash).
    /// </summary>
    public string? FromRef { get; init; }

    /// <summary>
    /// The ending reference (tag, branch, or commit hash). Defaults to HEAD.
    /// </summary>
    public string ToRef { get; init; } = "HEAD";

    /// <summary>
    /// Filter commits from this date onwards.
    /// </summary>
    public DateTimeOffset? FromDate { get; init; }

    /// <summary>
    /// Filter commits up to this date.
    /// </summary>
    public DateTimeOffset? ToDate { get; init; }

    /// <summary>
    /// Whether to include links to issues and pull requests.
    /// </summary>
    public bool IncludeLinks { get; init; } = true;

    /// <summary>
    /// Whether to include the list of contributors.
    /// </summary>
    public bool IncludeContributors { get; init; } = true;

    /// <summary>
    /// The repository URL for generating links.
    /// </summary>
    public string? RepositoryUrl { get; init; }

    /// <summary>
    /// Custom template name to use for generation.
    /// </summary>
    public string? TemplateName { get; init; }

    /// <summary>
    /// Categories of changes to exclude.
    /// </summary>
    public IReadOnlyList<string> ExcludeCategories { get; init; } = [];

    /// <summary>
    /// The path to the git repository.
    /// </summary>
    public string RepositoryPath { get; init; } = ".";

    /// <summary>
    /// Output file path (optional, defaults to console output).
    /// </summary>
    public string? OutputPath { get; init; }

    /// <summary>
    /// Optional system prompt override.
    /// </summary>
    public string? SystemPrompt { get; init; }
}
