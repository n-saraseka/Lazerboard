using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents the command sent to report a user
/// </summary>
public class ReportCommand
{
    /// <summary>
    /// The <see cref="User.Id"/>
    /// </summary>
    public int UserId { get; set; }
}