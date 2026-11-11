using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents a user
/// </summary>
public class ApiUser
{
    /// <summary>
    /// The user ID
    /// </summary>
    public int Id { get; set; }
    /// <summary>
    /// The username
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// The country code
    /// </summary>
    public string? CountryCode { get; set; }
    /// <summary>
    /// The user's country
    /// </summary>
    public Country? Country { get; set; }
    /// <summary>
    /// Whether the user is banned/deleted on osu! or not
    /// </summary>
    public bool IsRestricted { get; set; }
}