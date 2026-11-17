using System.Timers;

namespace Lazerboard.ExternalApis.Services.DirectApi;

public class DirectApiQueue : IWeightedQueue
{
    private readonly Lock _stateLock = new();
    private PriorityQueue<Request, decimal> _pendingRequests = new();
    
    private decimal _virtualTime;
    private const decimal MaxWeight = 2;

    private readonly Dictionary<decimal, decimal> _finishTimes = new();
    private decimal _lastAheadFinishTime;

    private int _availableTokens;
    private const int MaxTokens = 1;
    
    private readonly System.Timers.Timer _refillTimer;
    
    public DirectApiQueue(IConfiguration config)
    {
        var externalApisConfig = config.GetSection("ExternalApis");
        var osuApiConfig = externalApisConfig.GetSection("OsuDotDirect");
        var maxRpm = osuApiConfig.GetValue<int>("MaxRpm");
        var apiInterval = 60.0 / maxRpm;
        
        _refillTimer = new System.Timers.Timer(TimeSpan.FromSeconds(apiInterval).TotalMilliseconds);
        _refillTimer.Elapsed += OnRefillTimerActivated;
        _refillTimer.AutoReset = true;
        _refillTimer.Enabled = true;
        for (var i = 1; i <= MaxWeight; i++) _finishTimes[i] = 0;
    }

    /// <summary>
    /// Enqueue ahead of the queue and wait for available token
    /// </summary>
    /// <param name="token">The <see cref="CancellationToken"/></param>
    /// <returns>The request <see cref="Task"/> to await</returns>
    public Task EnqueueAndShiftTheRestAsync(CancellationToken token)
    {
        var completionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        lock (_stateLock)
        {
            var requestTime = 1 / MaxWeight;
            var shiftedItems = _pendingRequests.UnorderedItems.Select(i => (i.Element, i.Priority + requestTime + 1)).ToList();
            foreach (var priority in _finishTimes.Keys)
            {
                _finishTimes[priority] += requestTime + 1;
            }
            _pendingRequests = new PriorityQueue<Request, decimal>(shiftedItems);
            
            var lastAhead = _lastAheadFinishTime;
            var finishTime = lastAhead + requestTime;
            _lastAheadFinishTime = finishTime;
            var request = new Request
            {
                CompletionSource = completionSource,
                FinishTime = finishTime,
                CancellationToken = token
            };
            _pendingRequests.Enqueue(request, finishTime);
            GrantAvailableTokens();
        }
        
        return completionSource.Task;
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
            var request = new Request
            {
                CompletionSource = completionSource,
                FinishTime = finishTime,
                CancellationToken = token
            };
            _pendingRequests.Enqueue(request, finishTime);
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

    public void Dispose()
    {
        _refillTimer.Stop();
        _refillTimer.Dispose();
    }
}