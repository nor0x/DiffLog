using DiffLog.Services;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DiffLog.Commands;

/// <summary>
/// Command to list available tags in the repository.
/// </summary>
public class TagsCommand : AsyncCommand<TagsSettings>
{
    private readonly IGitService _gitService;

    public TagsCommand(IGitService gitService)
    {
        _gitService = gitService;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, TagsSettings settings, CancellationToken cancellationToken)
    {
        try
        {
            var repoPath = Path.GetFullPath(settings.RepositoryPath);

            if (!await _gitService.IsValidRepositoryAsync(repoPath))
            {
                AnsiConsole.MarkupLine("[red]Error:[/] The specified path is not a valid git repository.");
                return 1;
            }

            var tags = await _gitService.GetTagsAsync(repoPath);

            if (tags.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No tags found in the repository.[/]");
                return 0;
            }

            AnsiConsole.Write(new Rule("[blue]Available Tags[/]").LeftJustified());
            
            var table = new Table()
                .Border(TableBorder.Rounded)
                .AddColumn("#")
                .AddColumn("Tag");

            for (int i = 0; i < tags.Count; i++)
            {
                table.AddRow((i + 1).ToString(), tags[i]);
            }

            AnsiConsole.Write(table);

            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.WriteException(ex);
            return 1;
        }
    }
}

/// <summary>
/// Settings for the tags command.
/// </summary>
public class TagsSettings : CommandSettings
{
    [System.ComponentModel.Description("Path to the git repository. Defaults to current directory.")]
    [CommandOption("-p|--path <PATH>")]
    [System.ComponentModel.DefaultValue(".")]
    public string RepositoryPath { get; set; } = ".";
}
