using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents the API error state
/// </summary>
public enum ErrorState
{
    /// <summary>
    /// The user reported themselves
    /// </summary>
    SelfReport,
    /// <summary>
    /// The reported user is already restricted
    /// </summary>
    UserIsRestricted,
    /// <summary>
    /// There is an in-progress report of this user
    /// </summary>
    UserIsAlreadyReported,
    /// <summary>
    /// The reporting user made too many false reports recently
    /// </summary>
    TooManyFalseReports,
    /// <summary>
    /// The reporting user made too many reports recently
    /// </summary>
    TooManyReports,
    /// <summary>
    /// The reporting user is on a reporting cooldown
    /// </summary>
    OnCooldown
}