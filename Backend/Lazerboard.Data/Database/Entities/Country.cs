namespace Lazerboard.Data.Database.Entities;

public class Country : IEntity<string>
{
    /// <summary>
    /// The country code
    /// </summary>
    public string Id { get; set; }
    /// <summary>
    /// The country name
    /// </summary>
    public string Name { get; set; }
}