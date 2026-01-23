namespace DiffLog.Models;

/// <summary>
/// Represents information about a single git commit.
/// </summary>
public record CommitInfo
{
    /// <summary>
    /// The commit hash (SHA).
    /// </summary>
    public required string Hash { get; init; }

    /// <summary>
    /// The short commit hash.
    /// </summary>
    public required string ShortHash { get; init; }

    /// <summary>
    /// The commit message subject (first line).
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// The full commit message body.
    /// </summary>
    public string? Body { get; init; }

    /// <summary>
    /// The author name.
    /// </summary>
    public required string Author { get; init; }

    /// <summary>
    /// The author email.
    /// </summary>
    public required string AuthorEmail { get; init; }

    /// <summary>
    /// The commit date.
    /// </summary>
    public required DateTimeOffset Date { get; init; }

    /// <summary>
    /// Associated tags for this commit.
    /// </summary>
    public IReadOnlyList<string> Tags { get; init; } = [];

    /// <summary>
    /// List of files changed in this commit.
    /// </summary>
    public IReadOnlyList<string> ChangedFiles { get; init; } = [];
}
