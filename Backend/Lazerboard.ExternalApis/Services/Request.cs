namespace Lazerboard.ExternalApis.Services;

public class Request
{
    public required TaskCompletionSource CompletionSource { get; set; }
    public decimal FinishTime { get; set; }
    public CancellationToken CancellationToken { get; set; }
}