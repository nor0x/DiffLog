using Spectre.Console;

namespace DiffLog.Infrastructure;

public static class ConsoleBranding
{
    public static void WriteBanner(string? versionString, string? subtitle)
    {
        AnsiConsole.MarkupLine("[blue]  ┌┬┐┬┌─┐┌─┐┬  ┌─┐┌─┐  ┌─┐┬[/]");
        AnsiConsole.MarkupLine("[blue]   │││├─ ├─ │  │ ││ ┬  ├─┤│[/]");
        AnsiConsole.MarkupLine("[blue]  ─┴┘┴┴  ┴  ┴─┘└─┘└─┘  ┴ ┴┴[/]");

        if (!string.IsNullOrWhiteSpace(versionString))
        {
            AnsiConsole.MarkupLine($"[dim]{versionString,30}[/]");
        }

        AnsiConsole.WriteLine();

        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            AnsiConsole.MarkupLine($"[dim]{subtitle}[/]");
            AnsiConsole.WriteLine();
        }
    }
}
