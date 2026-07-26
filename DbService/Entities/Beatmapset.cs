namespace OsuScoreStats.DbService.Entities;

public class Beatmapset : IEntity<int>
{
    public int Id { get; set; }
    public required string Artist { get; set; }
    public required string Title { get; set; }
    public required string PreviewUrl { get; set; }
}