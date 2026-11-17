namespace Lazerboard.ExternalApis.Services;

public interface IWeightedQueue : IDisposable
{
    Task EnqueueAndShiftTheRestAsync(CancellationToken token);
    Task WaitForAvailableTokensAsync(decimal priority, CancellationToken token);
}