using System.Timers;

namespace Lazerboard.ExternalApis.Services.OsuApi;

/// <summary>
/// A simplistic implementation of WFQ that distributes the request throughput based on a request weighting system
/// </summary>
public class OsuApiQueue : IDisposable
{
    private readonly Lock _stateLock = new();
    private readonly PriorityQueue<Request, decimal> _pendingRequests = new();
    
    private decimal _virtualTime;
    private const decimal MaxWeight = 3;

    private readonly Dictionary<decimal, decimal> _finishTimes = new();

    private int _availableTokens;
    private const int MaxTokens = 1;
    
    private readonly System.Timers.Timer _refillTimer;
    
    public OsuApiQueue(IConfiguration config)
    {
        var externalApisConfig = config.GetSection("ExternalApis");
        var osuApiConfig = externalApisConfig.GetSection("OsuApi");
        var apiInterval = osuApiConfig.GetValue<double>("ApiInterval");
        
        _refillTimer = new System.Timers.Timer(TimeSpan.FromSeconds(apiInterval).TotalMilliseconds);
        _refillTimer.Elapsed += OnRefillTimerActivated;
        _refillTimer.AutoReset = true;
        _refillTimer.Enabled = true;
        for (var i = 1; i <= MaxTokens; i++) _finishTimes[i] = 0;
    }

    /// <summary>
    /// Wait for an available WFQ token
    /// </summary>
    /// <param name="priority">Priority number</param>
    /// <param name="token">A <see cref="CancellationToken"/></param>
    /// <returns>The request <see cref="Task"/> to await</returns>
    public Task WaitForAvailableTokensAsync(decimal priority, CancellationToken token)
    {
        var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        lock (_stateLock)
        {
            var requestTime = priority / MaxWeight;
            
            // In case there are many requests from the same queue and the virtual time hasn't shifted,
            // we use the last finish time from that priority queue
            var lastFinish = _finishTimes[priority];
            var startTime = Math.Max(_virtualTime, lastFinish);
            
            var finishTime = startTime + requestTime;
            _finishTimes[priority] = finishTime;
            _pendingRequests.Enqueue(new Request(completionSource, finishTime, token), finishTime);
            GrantAvailableTokens();
        }
        
        return completionSource.Task;
    }

    /// <summary>
    /// Grant available tokens to most recent request virtual time-wise
    /// </summary>
    private void GrantAvailableTokens()
    {
        while (_availableTokens >= 1 && _pendingRequests.TryDequeue(out var request, out _))
        {
            if (request.CompletionSource.Task.IsCompleted) continue;
            if (request.CancellationToken.IsCancellationRequested)
            {
                request.CompletionSource.TrySetCanceled(request.CancellationToken);
                continue;
            }
            _availableTokens--;
            _virtualTime = Math.Max(_virtualTime, request.FinishTime);
            request.CompletionSource.TrySetResult();
        }
    }
    
    private void OnRefillTimerActivated(object? source, ElapsedEventArgs e)
    {
        lock (_stateLock)
        {
            _availableTokens = Math.Min(MaxTokens, _availableTokens + 1);
            GrantAvailableTokens();
        }
    }

    /// <summary>
    /// A queue request
    /// </summary>
    /// <param name="completionSource">The <see cref="TaskCompletionSource"/> to track request completion</param>
    /// <param name="finishTime">The virtual finish time</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/></param>
    private class Request(TaskCompletionSource completionSource, decimal finishTime, CancellationToken cancellationToken)
    {
        public readonly TaskCompletionSource CompletionSource = completionSource;
        public readonly decimal FinishTime = finishTime;
        public readonly CancellationToken CancellationToken = cancellationToken;
    }

    public void Dispose()
    {
        _refillTimer.Stop();
        _refillTimer.Dispose();
    }
}