namespace Lazerboard.Api.Dtos;

public class ReportError
{
    public ErrorState Code { get; set; }
    public string Message { get; set; } = string.Empty;
}