namespace DiffLog.Models;

/// <summary>
/// Defines the target audience for release notes.
/// </summary>
public enum Audience
{
    /// <summary>
    /// Technical audience - includes implementation details, breaking changes, API updates.
    /// </summary>
    Developers,

    /// <summary>
    /// Non-technical audience - focuses on user-facing features and improvements.
    /// </summary>
    EndUsers,

    /// <summary>
    /// Brief, engaging content suitable for social media announcements.
    /// </summary>
    SocialMedia,

    /// <summary>
    /// Executive summary - high-level overview of changes and business impact.
    /// </summary>
    Executive
}
