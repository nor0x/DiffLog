namespace DiffLog.Models;

public class AiConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string? BaseUrl { get; set; }
    public string Model { get; set; } = "gpt-4o";
    public Dictionary<Audience, string> AudienceSystemPrompts { get; set; } = new();
}
