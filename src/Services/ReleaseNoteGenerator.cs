using System.Text;
using DiffLog.Models;
using Microsoft.Extensions.AI;

namespace DiffLog.Services;

/// <summary>
/// Generates release notes using AI via Microsoft.Extensions.AI.
/// </summary>
public class ReleaseNoteGenerator : IReleaseNoteGenerator
{
    private readonly IChatClient _chatClient;

    public ReleaseNoteGenerator(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<ReleaseNotes> GenerateAsync(
        IReadOnlyList<CommitInfo> commits,
        ReleaseNoteOptions options,
        CancellationToken cancellationToken = default)
    {
        if (commits.Count == 0)
        {
            return new ReleaseNotes
            {
                Title = "No Changes",
                Content = "No commits found for the specified range.",
                TargetAudience = options.Audience,
                Format = options.Format,
                Commits = commits
            };
        }

        var systemPrompt = BuildSystemPrompt(options);
        var userPrompt = BuildUserPrompt(commits, options);

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userPrompt)
        };

        var response = await _chatClient.GetResponseAsync(messages, cancellationToken: cancellationToken);
        var content = response.Text ?? "Failed to generate release notes.";

        var contributors = commits
            .Select(c => c.Author)
            .Distinct()
            .OrderBy(a => a)
            .ToList();

        var version = commits
            .SelectMany(c => c.Tags)
            .FirstOrDefault();

        return new ReleaseNotes
        {
            Title = $"Release Notes{(version != null ? $" - {version}" : "")}",
            Version = version,
            ReleaseDate = commits.Max(c => c.Date),
            Content = content,
            TargetAudience = options.Audience,
            Format = options.Format,
            Commits = commits,
            Contributors = contributors
        };
    }

    private static string BuildSystemPrompt(ReleaseNoteOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.SystemPrompt))
        {
            return options.SystemPrompt.Trim();
        }

        var audienceGuidelines = options.Audience switch
        {
            Audience.Developers => """
                You are generating release notes for a technical developer audience.
                Include:
                - Breaking changes with migration guides
                - API changes and deprecations
                - New features with code examples where relevant
                - Bug fixes with technical details
                - Performance improvements with metrics if available
                - Dependency updates
                Use technical language and be precise about changes.
                """,
            Audience.EndUsers => """
                You are generating release notes for end users who are not developers.
                Include:
                - New features explained in simple terms
                - Improvements to existing functionality
                - Fixed issues that users may have experienced
                - Any changes to the user interface
                Avoid technical jargon. Focus on how changes benefit the user.
                """,
            Audience.SocialMedia => """
                You are generating a brief, engaging announcement suitable for social media.
                - Keep it concise (suitable for Twitter/X, LinkedIn)
                - Highlight the most exciting features
                - Use emojis appropriately
                - Include relevant hashtags
                - Make it shareable and engaging
                """,
            Audience.Executive => """
                You are generating an executive summary of the release.
                Include:
                - High-level overview of major changes
                - Business impact and value delivered
                - Key metrics and improvements
                - Strategic alignment notes
                Keep it brief and focus on outcomes rather than technical details.
                """,
            _ => "Generate professional release notes."
        };

        var formatGuidelines = options.Format switch
        {
            OutputFormat.Markdown => "Format the output as Markdown with proper headers, lists, and formatting.",
            OutputFormat.Html => "Format the output as clean, semantic HTML.",
            OutputFormat.PlainText => "Format the output as plain text without any markup.",
            OutputFormat.Json => "Format the output as a JSON object with structured sections.",
            _ => "Format the output appropriately."
        };

        return $"""
            {audienceGuidelines}

            {formatGuidelines}

            Group related changes together logically. Categorize changes into sections like:
            - Features / New
            - Improvements / Enhancements  
            - Bug Fixes
            - Breaking Changes (if any)
            - Other Changes

            Be concise but informative. Do not make up information that isn't in the commit data.
            """;
    }

    private static string BuildUserPrompt(IReadOnlyList<CommitInfo> commits, ReleaseNoteOptions options)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("Generate release notes from the following commits:");
        sb.AppendLine();

        foreach (var commit in commits)
        {
            sb.AppendLine($"Commit: {commit.ShortHash}");
            sb.AppendLine($"Date: {commit.Date:yyyy-MM-dd}");
            sb.AppendLine($"Author: {commit.Author}");
            sb.AppendLine($"Subject: {commit.Subject}");
            
            if (!string.IsNullOrWhiteSpace(commit.Body))
            {
                sb.AppendLine($"Body: {commit.Body}");
            }
            
            if (commit.Tags.Count > 0)
            {
                sb.AppendLine($"Tags: {string.Join(", ", commit.Tags)}");
            }
            
            if (commit.ChangedFiles.Count > 0)
            {
                sb.AppendLine($"Changed Files: {string.Join(", ", commit.ChangedFiles.Take(10))}");
                if (commit.ChangedFiles.Count > 10)
                {
                    sb.AppendLine($"  ... and {commit.ChangedFiles.Count - 10} more files");
                }
            }
            
            sb.AppendLine();
        }

        if (options.IncludeLinks && !string.IsNullOrEmpty(options.RepositoryUrl))
        {
            sb.AppendLine($"Repository URL for links: {options.RepositoryUrl}");
        }

        if (options.IncludeContributors)
        {
            var contributors = commits.Select(c => c.Author).Distinct().OrderBy(a => a);
            sb.AppendLine($"Contributors: {string.Join(", ", contributors)}");
        }

        if (options.ExcludeCategories.Count > 0)
        {
            sb.AppendLine($"Exclude these categories: {string.Join(", ", options.ExcludeCategories)}");
        }

        return sb.ToString();
    }
}
