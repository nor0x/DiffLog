using DiffLog.Models;

namespace DiffLog.Services;

/// <summary>
/// Interface for generating release notes using AI.
/// </summary>
public interface IReleaseNoteGenerator
{
    /// <summary>
    /// Generates release notes from a list of commits.
    /// </summary>
    Task<ReleaseNotes> GenerateAsync(
        IReadOnlyList<CommitInfo> commits,
        ReleaseNoteOptions options,
        CancellationToken cancellationToken = default);
}
