using System.Reflection;

namespace DiffLog.Infrastructure;

public static class AppInfo
{
    private static readonly Assembly Assembly = Assembly.GetEntryAssembly() ?? typeof(AppInfo).Assembly;

    public static string Version => GetVersionString();

    public static string RepositoryUrl => GetMetadata("RepositoryUrl") ?? "unknown";

    public static string CommitHash => GetMetadata("CommitHash") ?? GetMetadata("SourceRevisionId") ?? "unknown";

    public static string CommitHashShort
    {
        get
        {
            var hash = CommitHash;
            return hash.Length > 8 ? hash[..8] : hash;
        }
    }

    private static string GetVersionString()
    {
        var informational = Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (!string.IsNullOrWhiteSpace(informational))
        {
            return informational.StartsWith('v') ? informational : $"v{informational}";
        }

        var version = Assembly.GetName().Version;
        if (version == null)
        {
            return "";
        }

        return $"v{version.Major}.{version.Minor}.{version.Build}";
    }

    private static string? GetMetadata(string key)
    {
        return Assembly
            .GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(attribute => string.Equals(attribute.Key, key, StringComparison.OrdinalIgnoreCase))
            ?.Value;
    }
}
