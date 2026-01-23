namespace DiffLog.Models;

/// <summary>
/// Represents generated release notes.
/// </summary>
public record ReleaseNotes
{
    /// <summary>
    /// The title of the release.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// The version or tag name.
    /// </summary>
    public string? Version { get; init; }

    /// <summary>
    /// The release date.
    /// </summary>
    public DateTimeOffset ReleaseDate { get; init; } = DateTimeOffset.Now;

    /// <summary>
    /// The formatted release notes content.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// The target audience these notes were generated for.
    /// </summary>
    public Audience TargetAudience { get; init; }

    /// <summary>
    /// The output format of the content.
    /// </summary>
    public OutputFormat Format { get; init; }

    /// <summary>
    /// The commits included in this release.
    /// </summary>
    public IReadOnlyList<CommitInfo> Commits { get; init; } = [];

    /// <summary>
    /// List of contributors.
    /// </summary>
    public IReadOnlyList<string> Contributors { get; init; } = [];
}
