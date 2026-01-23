using System.Diagnostics;
using System.Globalization;
using DiffLog.Models;

namespace DiffLog.Services;

/// <summary>
/// Git service implementation using git CLI commands.
/// </summary>
public class GitService : IGitService
{
    private const string CommitSeparator = "---COMMIT_SEPARATOR---";
    private const string FieldSeparator = "---FIELD_SEPARATOR---";

    public async Task<IReadOnlyList<CommitInfo>> GetCommitsAsync(
        string repositoryPath,
        string? fromRef = null,
        string? toRef = "HEAD",
        DateTimeOffset? fromDate = null,
        DateTimeOffset? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var range = string.IsNullOrEmpty(fromRef) ? toRef : $"{fromRef}..{toRef}";
        
        // Format: hash, short hash, subject, body, author, email, date (ISO)
        var format = $"%H{FieldSeparator}%h{FieldSeparator}%s{FieldSeparator}%b{FieldSeparator}%an{FieldSeparator}%ae{FieldSeparator}%aI{CommitSeparator}";
        
        var args = $"log {range} --pretty=format:\"{format}\"";
        
        if (fromDate.HasValue)
        {
            args += $" --after=\"{fromDate.Value:yyyy-MM-dd}\"";
        }
        
        if (toDate.HasValue)
        {
            args += $" --before=\"{toDate.Value:yyyy-MM-dd}\"";
        }

        var output = await RunGitCommandAsync(repositoryPath, args, cancellationToken);
        
        if (string.IsNullOrWhiteSpace(output))
        {
            return [];
        }

        var commits = new List<CommitInfo>();
        var commitStrings = output.Split(CommitSeparator, StringSplitOptions.RemoveEmptyEntries);

        foreach (var commitString in commitStrings)
        {
            var trimmed = commitString.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                continue;
            }

            var fields = trimmed.Split(FieldSeparator);
            if (fields.Length < 7)
            {
                continue;
            }

            var commit = new CommitInfo
            {
                Hash = fields[0].Trim(),
                ShortHash = fields[1].Trim(),
                Subject = fields[2].Trim(),
                Body = string.IsNullOrWhiteSpace(fields[3]) ? null : fields[3].Trim(),
                Author = fields[4].Trim(),
                AuthorEmail = fields[5].Trim(),
                Date = DateTimeOffset.Parse(fields[6].Trim(), CultureInfo.InvariantCulture),
                Tags = await GetTagsForCommitAsync(repositoryPath, fields[0].Trim(), cancellationToken),
                ChangedFiles = await GetChangedFilesAsync(repositoryPath, fields[0].Trim(), cancellationToken)
            };

            commits.Add(commit);
        }

        return commits;
    }

    public async Task<IReadOnlyList<string>> GetTagsAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var output = await RunGitCommandAsync(repositoryPath, "tag --sort=-creatordate", cancellationToken);
        
        if (string.IsNullOrWhiteSpace(output))
        {
            return [];
        }

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList();
    }

    public async Task<string?> GetRemoteUrlAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        var output = await RunGitCommandAsync(repositoryPath, "remote get-url origin", cancellationToken);
        return string.IsNullOrWhiteSpace(output) ? null : NormalizeRemoteUrl(output.Trim());
    }

    public async Task<bool> IsValidRepositoryAsync(
        string repositoryPath,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var output = await RunGitCommandAsync(repositoryPath, "rev-parse --is-inside-work-tree", cancellationToken);
            return output?.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) ?? false;
        }
        catch
        {
            return false;
        }
    }

    private async Task<IReadOnlyList<string>> GetTagsForCommitAsync(
        string repositoryPath,
        string commitHash,
        CancellationToken cancellationToken)
    {
        var output = await RunGitCommandAsync(repositoryPath, $"tag --points-at {commitHash}", cancellationToken);
        
        if (string.IsNullOrWhiteSpace(output))
        {
            return [];
        }

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList();
    }

    private async Task<IReadOnlyList<string>> GetChangedFilesAsync(
        string repositoryPath,
        string commitHash,
        CancellationToken cancellationToken)
    {
        var output = await RunGitCommandAsync(repositoryPath, $"diff-tree --no-commit-id --name-only -r {commitHash}", cancellationToken);
        
        if (string.IsNullOrWhiteSpace(output))
        {
            return [];
        }

        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim())
            .ToList();
    }

    private static string NormalizeRemoteUrl(string url)
    {
        // Convert SSH URLs to HTTPS URLs for link generation
        if (url.StartsWith("git@"))
        {
            // git@github.com:user/repo.git -> https://github.com/user/repo
            url = url.Replace("git@", "https://")
                     .Replace(".com:", ".com/")
                     .Replace(".org:", ".org/");
        }

        // Remove .git suffix
        if (url.EndsWith(".git"))
        {
            url = url[..^4];
        }

        return url;
    }

    private static async Task<string> RunGitCommandAsync(
        string workingDirectory,
        string arguments,
        CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "git",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);

        return output;
    }
}
