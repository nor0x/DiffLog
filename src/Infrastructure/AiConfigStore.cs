using System.Text.Json;
using DiffLog.Models;

namespace DiffLog.Infrastructure;

public static class AiConfigStore
{
    private const string ConfigDirectoryName = "DiffLog";
    private const string ConfigFileName = "config.json";

    public static string GetConfigPath()
    {
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrWhiteSpace(basePath))
        {
            basePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        return Path.Combine(basePath, ConfigDirectoryName, ConfigFileName);
    }

    public static AiConfiguration? Load()
    {
        var path = GetConfigPath();
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<AiConfiguration>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public static void Save(AiConfiguration configuration)
    {
        var path = GetConfigPath();
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        File.WriteAllText(path, json);
    }
}
