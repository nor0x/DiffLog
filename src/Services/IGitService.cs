using DiffLog.Models;

namespace DiffLog.Services;

/// <summary>
/// Interface for interacting with git repositories.
/// </summary>
public interface IGitService
{
    /// <summary>
    /// Gets the list of commits between two references.
    /// </summary>
    Task<IReadOnlyList<CommitInfo>> GetCommitsAsync(
        string repositoryPath,
        string? fromRef = null,
        string? toRef = "HEAD",
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all tags in the repository.
    /// </summary>
    Task<IReadOnlyList<string>> GetTagsAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the remote URL of the repository.
    /// </summary>
    Task<string?> GetRemoteUrlAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates that the path is a valid git repository.
    /// </summary>
    Task<bool> IsValidRepositoryAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default);
}
