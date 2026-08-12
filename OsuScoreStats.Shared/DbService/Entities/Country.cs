namespace OsuScoreStats.Shared.DbService.Entities;

public class Country : IEntity<string>
{
    public string Id { get; set; }
    public string Name { get; set; }
}