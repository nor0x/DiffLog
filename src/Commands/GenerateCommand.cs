using System.Globalization;
using DiffLog.Infrastructure;
using DiffLog.Models;
using DiffLog.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DiffLog.Commands;

/// <summary>
/// Command to generate release notes from git history.
/// </summary>
public class GenerateCommand : AsyncCommand<GenerateSettings>
{
    private readonly IGitService _gitService;
    private readonly IReleaseNoteGenerator _releaseNoteGenerator;

    public GenerateCommand(IGitService gitService, IReleaseNoteGenerator releaseNoteGenerator)
    {
        _gitService = gitService;
        _releaseNoteGenerator = releaseNoteGenerator;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GenerateSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            if (!string.IsNullOrEmpty(settings.SystemPrompt) && !string.IsNullOrEmpty(settings.SystemPromptFile))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] Provide either --system-prompt or --system-prompt-file, not both.");
                return 1;
            }

            if (!string.IsNullOrEmpty(settings.SystemPromptFile) && !File.Exists(settings.SystemPromptFile))
            {
                AnsiConsole.MarkupLine($"[red]Error:[/] System prompt file not found: {settings.SystemPromptFile}");
                return 1;
            }

            // Resolve repository path
            var repoPath = Path.GetFullPath(settings.RepositoryPath);

            // Validate repository
            if (!await _gitService.IsValidRepositoryAsync(repoPath))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] The specified path is not a valid git repository.");
                return 1;
            }

            // Interactive mode
            if (settings.Interactive)
            {
                settings = await RunInteractiveModeAsync(settings, repoPath);
            }

            // Build options
            var options = await BuildOptionsAsync(settings, repoPath);

            // Show configuration
            DisplayConfiguration(options);

            // Generate release notes
            var releaseNotes = await GenerateReleaseNotesAsync(options);

            if (releaseNotes == null)
            {
                return 1;
            }

            // Output results
            await OutputReleaseNotesAsync(releaseNotes, options);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }

    public static void DisplayWelcome()
    {
        ConsoleBranding.WriteBanner(AppInfo.Version, "Generate AI-powered release notes from git history");
    }

    private async Task<GenerateSettings> RunInteractiveModeAsync(GenerateSettings settings, string repoPath)
    {
        DisplayWelcome();

        // Get available tags for selection
        var tags = await _gitService.GetTagsAsync(repoPath);

        // Select from reference
        if (tags.Count > 0)
        {
            var fromChoices = new List<string> { "(All commits)" };
            fromChoices.AddRange(tags);

            var fromSelection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select [green]starting point[/] (from):")
                    .PageSize(10)
                    .AddChoices(fromChoices));

            settings.FromRef = fromSelection == "(All commits)" ? null : fromSelection;

            // Select to reference
            var toChoices = new List<string> { "HEAD (latest)" };
            toChoices.AddRange(tags.Where(t => t != settings.FromRef));

            var toSelection = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select [green]ending point[/] (to):")
                    .PageSize(10)
                    .AddChoices(toChoices));

            settings.ToRef = toSelection == "HEAD (latest)" ? "HEAD" : toSelection;
        }

        if (string.IsNullOrWhiteSpace(settings.SystemPrompt) && string.IsNullOrWhiteSpace(settings.SystemPromptFile))
        {
            // Select audience
            settings.Audience = AnsiConsole.Prompt(
                new SelectionPrompt<Audience>()
                    .Title("Select [green]target audience[/]:")
                    .AddChoices(Enum.GetValues<Audience>())
                    .UseConverter(a => a switch
                    {
                        Audience.Developers => "[yellow]:man_technologist:[/] Developers - Technical details, API changes, breaking changes",
                        Audience.EndUsers => "[yellow]:bust_in_silhouette:[/] End Users - User-friendly feature descriptions",
                        Audience.SocialMedia => "[yellow]:mobile_phone:[/] Social Media - Brief, engaging announcements",
                        Audience.Executive => "[yellow]:bar_chart:[/] Executive - High-level business impact summary",
                        _ => a.ToString()
                    }));
        }

        if (string.IsNullOrWhiteSpace(settings.SystemPrompt) && string.IsNullOrWhiteSpace(settings.SystemPromptFile))
        {
            // Select format
            settings.Format = AnsiConsole.Prompt(
                new SelectionPrompt<OutputFormat>()
                    .Title("Select [green]output format[/]:")
                    .AddChoices(Enum.GetValues<OutputFormat>())
                    .UseConverter(f => f switch
                    {
                        OutputFormat.Markdown => "[yellow]:memo:[/] Markdown",
                        OutputFormat.Html => "[yellow]:globe_with_meridians:[/] HTML",
                        OutputFormat.PlainText => "[yellow]:page_facing_up:[/] Plain Text",
                        OutputFormat.Json => "[yellow]:clipboard:[/] JSON",
                        _ => f.ToString()
                    }));
        }

        // Output file
        var defaultPath = BuildDefaultOutputPath(repoPath, settings.Format);

        settings.OutputPath = AnsiConsole.Ask(
            "Output file path:",
            defaultPath);

        settings.PrintToConsole = AnsiConsole.Confirm("Also print to console?", false);

        AnsiConsole.WriteLine();

        return settings;
    }

    private async Task<ReleaseNoteOptions> BuildOptionsAsync(GenerateSettings settings, string repoPath)
    {
        var repositoryUrl = settings.RepositoryUrl ?? await _gitService.GetRemoteUrlAsync(repoPath);

        DateTimeOffset? fromDate = null;
        DateTimeOffset? toDate = null;

        if (!string.IsNullOrEmpty(settings.FromDate) && 
            DateTimeOffset.TryParse(settings.FromDate, CultureInfo.InvariantCulture, out var parsedFromDate))
        {
            fromDate = parsedFromDate;
        }

        if (!string.IsNullOrEmpty(settings.ToDate) && 
            DateTimeOffset.TryParse(settings.ToDate, CultureInfo.InvariantCulture, out var parsedToDate))
        {
            toDate = parsedToDate;
        }

        var excludeCategories = string.IsNullOrEmpty(settings.ExcludeCategories)
            ? []
            : settings.ExcludeCategories.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

        return new ReleaseNoteOptions
        {
            RepositoryPath = repoPath,
            FromRef = settings.FromRef,
            ToRef = settings.ToRef,
            Audience = settings.Audience,
            Format = settings.Format,
            IncludeLinks = !settings.NoLinks,
            IncludeContributors = !settings.NoContributors,
            RepositoryUrl = repositoryUrl,
            FromDate = fromDate,
            ToDate = toDate,
            ExcludeCategories = excludeCategories,
            OutputPath = string.IsNullOrWhiteSpace(settings.OutputPath)
                ? BuildDefaultOutputPath(repoPath, settings.Format)
                : settings.OutputPath,
            PrintToConsole = settings.PrintToConsole,
            SystemPrompt = await ResolveSystemPromptAsync(settings)
        };
    }

    private static async Task<string?> ResolveSystemPromptAsync(GenerateSettings settings)
    {
        if (!string.IsNullOrWhiteSpace(settings.SystemPrompt))
        {
            return settings.SystemPrompt.Trim();
        }

        if (!string.IsNullOrWhiteSpace(settings.SystemPromptFile))
        {
            var content = await File.ReadAllTextAsync(settings.SystemPromptFile);
            var trimmed = content.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }

        var config = AiConfigStore.Load();
        if (config == null)
        {
            return null;
        }

        return config.AudienceSystemPrompts.TryGetValue(settings.Audience, out var prompt)
            ? prompt
            : null;
    }

    private static void DisplayConfiguration(ReleaseNoteOptions options)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Setting")
            .AddColumn("Value");

        table.AddRow("Repository", options.RepositoryPath);
        table.AddRow("From", options.FromRef ?? "(beginning)");
        table.AddRow("To", options.ToRef);
        table.AddRow("Audience", options.Audience.ToString());
        table.AddRow("Format", options.Format.ToString());
        
        if (options.FromDate.HasValue)
            table.AddRow("From Date", options.FromDate.Value.ToString("yyyy-MM-dd"));
        
        if (options.ToDate.HasValue)
            table.AddRow("To Date", options.ToDate.Value.ToString("yyyy-MM-dd"));
        
        if (!string.IsNullOrEmpty(options.OutputPath))
            table.AddRow("Output", options.OutputPath);

        if (options.PrintToConsole)
            table.AddRow("Console", "Yes");

        if (!string.IsNullOrWhiteSpace(options.SystemPrompt))
            table.AddRow("System Prompt", "Custom");

        AnsiConsole.Write(new Panel(table)
            .Header("[blue]Configuration[/]")
            .Border(BoxBorder.Rounded));
        
        AnsiConsole.WriteLine();
    }

    private async Task<ReleaseNotes?> GenerateReleaseNotesAsync(ReleaseNoteOptions options)
    {
        return await AnsiConsole.Status()
            .Spinner(Spinner.Known.Toggle9)
            .SpinnerStyle(Style.Parse("blue"))
            .StartAsync("Generating release notes...", async ctx =>
            {
                // Fetch commits
                ctx.Status("Fetching commits from git history...");
                
                var commits = await _gitService.GetCommitsAsync(
                    options.RepositoryPath,
                    options.FromRef,
                    options.ToRef,
                    options.FromDate,
                    options.ToDate);

                if (commits.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]Warning:[/] No commits found for the specified range.");
                    return null;
                }

                AnsiConsole.MarkupLine($"[green]Found {commits.Count} commit(s)[/]");

                // Generate with AI
                ctx.Status("Generating release notes with AI...");
                
                var releaseNotes = await _releaseNoteGenerator.GenerateAsync(commits, options);

                AnsiConsole.MarkupLine("[green]✓[/] Release notes generated successfully!");
                
                return releaseNotes;
            });
    }

    private static async Task OutputReleaseNotesAsync(ReleaseNotes releaseNotes, ReleaseNoteOptions options)
    {
        // Save to file if specified
        if (!string.IsNullOrEmpty(options.OutputPath))
        {
            await File.WriteAllTextAsync(options.OutputPath, releaseNotes.Content);
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[green]✓[/] Saved to [link]{options.OutputPath}[/]");
        }

        if (!options.PrintToConsole)
        {
            return;
        }

        AnsiConsole.WriteLine();

        // Display release notes
        var panel = new Panel(new Text(releaseNotes.Content))
            .Header(Markup.Escape(releaseNotes.Title))
            .Border(BoxBorder.Double)
            .Expand();

        AnsiConsole.Write(panel);

        // Show stats
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule("[dim]Statistics[/]").LeftJustified());
        AnsiConsole.MarkupLine($"  [yellow]:bar_chart:[/] Commits: [blue]{releaseNotes.Commits.Count}[/]");
        AnsiConsole.MarkupLine($"  [yellow]:busts_in_silhouette:[/] Contributors: [blue]{releaseNotes.Contributors.Count}[/]");
        AnsiConsole.MarkupLine($"  [yellow]:bullseye:[/] Audience: [blue]{releaseNotes.TargetAudience}[/]");
        AnsiConsole.MarkupLine($"  [yellow]:page_facing_up:[/] Format: [blue]{releaseNotes.Format}[/]");
    }

    private static string BuildDefaultOutputPath(string repoPath, OutputFormat format)
    {
        var defaultName = $"difflog-{DateTime.Now:yyyy-MM-dd}";
        var extension = format switch
        {
            OutputFormat.Markdown => ".md",
            OutputFormat.Html => ".html",
            OutputFormat.Json => ".json",
            _ => ".txt"
        };

        return Path.Combine(repoPath, $"{defaultName}{extension}");
    }
}
