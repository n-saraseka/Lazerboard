namespace OsuScoreStats.DbService.Entities;

public class Beatmapset : IEntity<int>
{
    public int Id { get; set; }
    public string Artist { get; set; }
    public string Title { get; set; }
    public string PreviewUrl { get; set; }
    public List<Beatmap> Beatmaps { get; set; }
}