namespace Lazerboard.Data.Database.Entities;

public class Beatmapset : IEntity<int>
{
    public int Id { get; set; }
    public string Artist { get; set; }
    public string Title { get; set; }
    public string Creator { get; set; }
    public int? UserId { get; set; }
    public User? User { get; set; }
    public DateTimeOffset? RankedDate { get; set; }
    /// <summary>
    /// Set when beatmapset is added to the DB
    /// through the main queue (ranked beatmapsets available through the osu! API search endpoint)
    /// </summary>
    public DateTimeOffset? MainStartedProcessingAt { get; set; }
    /// <summary>
    /// Set when all leaderboards in the beatmapset are added to the DB
    /// through the main queue (ranked beatmapsets available through the osu! API search endpoint)
    /// </summary>
    public DateTimeOffset? MainFinishedProcessingAt { get; set; }
    /// <summary>
    /// Set when beatmapset is added to the DB
    /// through the secondary queue (ranked beatmapsets not available through the osu! API search endpoint)
    /// </summary>
    public DateTimeOffset? SecondaryStartedProcessingAt { get; set; }
    /// <summary>
    /// Set when all leaderboards in the beatmapset to the DB
    /// through the secondary queue (ranked beatmapsets not available through the osu! API search endpoint)
    /// </summary>
    public DateTimeOffset? SecondaryFinishedProcessingAt { get; set; }
    /// <summary>
    /// Set when a rescan of the beatmapset is started
    /// </summary>
    public DateTimeOffset? StartedScanningAt { get; set; }
    /// <summary>
    /// Set when all leaderboards in the beatmapset have been rescanned
    /// </summary>
    public DateTimeOffset? FinishedScanningAt { get; set; }
}