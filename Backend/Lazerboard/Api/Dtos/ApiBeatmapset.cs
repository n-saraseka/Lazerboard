namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents a beatmapset
/// </summary>
public class ApiBeatmapset
{
    /// <summary>
    /// The beatmapset ID
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The song artist
    /// </summary>
    public string Artist { get; set; }
    /// <summary>
    /// The song title
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// The user's name when they created the map
    /// </summary>
    public string Creator { get; set; }
    /// <summary>
    /// The user ID
    /// </summary>
    public int? UserId { get; set; }
    /// <summary>
    /// The user
    /// </summary>
    public ApiUser? User { get; set; }
    /// <summary>
    /// The beatmapset ranked date
    /// </summary>
    public DateTimeOffset? RankedDate { get; set; }
}