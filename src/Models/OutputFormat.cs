namespace DiffLog.Models;

/// <summary>
/// Defines the output format for release notes.
/// </summary>
public enum OutputFormat
{
    /// <summary>
    /// Markdown format - suitable for GitHub, documentation sites.
    /// </summary>
    Markdown,

    /// <summary>
    /// HTML format - suitable for web pages and emails.
    /// </summary>
    Html,

    /// <summary>
    /// Plain text format - suitable for simple text files and logs.
    /// </summary>
    PlainText,

    /// <summary>
    /// JSON format - suitable for programmatic consumption.
    /// </summary>
    Json
}
