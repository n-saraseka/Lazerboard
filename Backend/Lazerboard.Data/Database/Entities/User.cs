namespace Lazerboard.Data.Database.Entities;

public class User : IEntity<int>
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string? CountryCode { get; set; }
    public Country? Country { get; set; }
    public bool IsRestricted { get; set; }
    /// <summary>
    /// Reflects whether user logged in with osu! on the website or not
    /// </summary>
    public bool IsRegistered { get; set; }
    public DateTimeOffset? RegisteredAt { get; set; }
    public DateTime? LastCheckedAt { get; set; }
    public DateTime? LastScannedAt { get; set; }
}