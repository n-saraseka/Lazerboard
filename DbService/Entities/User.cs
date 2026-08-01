namespace OsuScoreStats.DbService.Entities;

public class User : IEntity<int>
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string CountryCode { get; set; }
    public Country Country { get; set; }
}