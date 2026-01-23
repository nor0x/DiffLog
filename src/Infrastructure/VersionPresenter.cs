using Spectre.Console;

namespace DiffLog.Infrastructure;

public static class VersionPresenter
{
    public static void Show()
    {
        ConsoleBranding.WriteBanner(AppInfo.Version, "Generate AI-powered release notes from git history");

        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Info")
            .AddColumn("Value");

        table.AddRow("Repository", AppInfo.RepositoryUrl);
        table.AddRow("Commit", AppInfo.CommitHashShort);

        AnsiConsole.Write(new Panel(table)
            .Header("[blue]Build Info[/]")
            .Border(BoxBorder.Rounded));

        AnsiConsole.WriteLine();
    }
}
