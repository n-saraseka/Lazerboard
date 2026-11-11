using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents the rank distribution of this user
/// </summary>
public class RankStats
{
    /// <summary>
    /// The maximum rank for this bracket
    /// </summary>
    public int RankBound { get; set; }
    /// <summary>
    /// Number of scores within this rank bracket
    /// </summary>
    public int Count { get; set; }
}