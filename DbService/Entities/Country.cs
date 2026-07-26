using System.ComponentModel.DataAnnotations;

namespace OsuScoreStats.DbService.Entities;

public class Country : IEntity<string>
{
    public string Id { get; set; }
    public required string Name { get; set; }
}