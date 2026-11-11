namespace Lazerboard.Api.Dtos;

/// <summary>
/// Represents an error returned after trying to make a report
/// </summary>
public class ReportError
{
    /// <summary>
    /// The <see cref="ErrorState"/>
    /// </summary>
    public ErrorState Code { get; set; }
    /// <summary>
    /// The error message
    /// </summary>
    public string Message { get; set; } = string.Empty;
}