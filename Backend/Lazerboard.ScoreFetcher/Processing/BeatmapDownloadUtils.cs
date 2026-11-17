using Lazerboard.Data.ApiFetchers;
using Microsoft.Extensions.Logging;
using osu.Framework.Extensions;

namespace Lazerboard.ScoreFetcher.Processing;

public class BeatmapDownloadUtils(ILogger<BeatmapDownloadUtils> logger,
    IOsuApiFetcher osuApiFetcher,
    IDirectApiFetcher directApiFetcher,
    INekohaApiFetcher nekohaApiFetcher) : IBeatmapDownloadUtils
{
    private const int MaxDownloadAttempts = 5;
    private readonly TimeSpan _downloadTimeout = TimeSpan.FromSeconds(5);
    
    /// <summary>
    /// Try downloading the .osu file from either osu.direct or osu! API
    /// </summary>
    /// <param name="beatmapId">The beatmap ID</param>
    /// <param name="filename">The .osu filename</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The filename, if download was a success</returns>
    /// <exception cref="TimeoutException">If the method couldn't download the beatmap from both providers after some amount of attempts</exception>
    public async Task<string> TryDownloadBeatmapAsync(int beatmapId, string filename, CancellationToken stoppingToken)
    {
        var attempts = 0; 
        // We assume the beatmap file is invalid as it has to be retrieved and verified first
        
        while (attempts < MaxDownloadAttempts)
        {
            var isInvalidFile = CheckIfFileIsInvalid(filename); // Download in case the file is empty or doesn't exist
            if (!isInvalidFile) return filename;
            
            try
            {
                return await DownloadFromNekohaAsync(beatmapId, filename, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Download from Nekoha failed for Beatmap ID {id}", beatmapId);
            }
            try
            {
                return await DownloadFromDotDirectAsync(beatmapId, filename, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Download from osu.direct failed for Beatmap ID {id}", beatmapId);
            }
            try
            {
                return await DownloadFromOsuApiAsync(beatmapId, filename, stoppingToken);
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Download from osu! API failed for Beatmap ID {id}", beatmapId);
            }
            logger.Log(LogLevel.Error, "Download from both providers failed for Beatmap ID {id}, attempt no. {attempt}", beatmapId, attempts);
            attempts++;
        }
        
        throw new TimeoutException(
            $"Failed to get beatmap {beatmapId} after {MaxDownloadAttempts} attempts");
    }

    private async Task<string> DownloadFromNekohaAsync(int beatmapId, string filename, CancellationToken stoppingToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeoutCts.CancelAfter(_downloadTimeout);
        
        try
        {
            await using var stream = await nekohaApiFetcher.DownloadBeatmapAsync(beatmapId, stoppingToken);
            var bytes = await stream.ReadAllRemainingBytesToArrayAsync(stoppingToken);
            await File.WriteAllBytesAsync(filename, bytes, stoppingToken);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Timeout while trying to download beatmap {beatmapId}");
        }
        
        var isInvalidFile = CheckIfFileIsInvalid(filename);
        if (!isInvalidFile) return filename;
        throw new IOException($"File {filename} is invalid");
    }

    /// <summary>
    /// Download .osu file from osu.direct
    /// </summary>
    /// <param name="beatmapId">The beatmap ID</param>
    /// <param name="filename">The filename</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The filename, if download was a success</returns>
    /// <exception cref="IOException">If the downloaded file is invalid (empty or something else failed</exception>
    private async Task<string> DownloadFromDotDirectAsync(int beatmapId, string filename,
        CancellationToken stoppingToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeoutCts.CancelAfter(_downloadTimeout);
        
        try
        {
            await using var stream = await directApiFetcher.DownloadBeatmapAsync(beatmapId, stoppingToken);
            var bytes = await stream.ReadAllRemainingBytesToArrayAsync(stoppingToken);
            await File.WriteAllBytesAsync(filename, bytes, stoppingToken);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Timeout while trying to download beatmap {beatmapId}");
        }
        
        var isInvalidFile = CheckIfFileIsInvalid(filename);
        if (!isInvalidFile) return filename;
        throw new IOException($"File {filename} is invalid");
    }

    /// <summary>
    /// Download .osu file from osu! API
    /// </summary>
    /// <param name="beatmapId">The beatmap ID</param>
    /// <param name="filename">The filename</param>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/></param>
    /// <returns>The filename, if download was a success</returns>
    /// <exception cref="IOException">If the downloaded file is invalid (empty or something else failed</exception>
    private async Task<string> DownloadFromOsuApiAsync(int beatmapId, string filename, CancellationToken stoppingToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        timeoutCts.CancelAfter(_downloadTimeout);
        
        try
        {
            await using var stream = await osuApiFetcher.DownloadBeatmapAsync(beatmapId, stoppingToken);
            var bytes = await stream.ReadAllRemainingBytesToArrayAsync(stoppingToken);
            await File.WriteAllBytesAsync(filename, bytes, stoppingToken);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested && !stoppingToken.IsCancellationRequested)
        {
            throw new TimeoutException($"Timeout while trying to download beatmap {beatmapId}");
        }
        
        var isInvalidFile = CheckIfFileIsInvalid(filename);
        if (!isInvalidFile) return filename;
        throw new IOException($"File {filename} is invalid");
    }

    /// <summary>
    /// Check if file is invalid.
    /// </summary>
    /// <param name="filename">The filename</param>
    /// <returns>True if file length is 0, false otherwise</returns>
    private bool CheckIfFileIsInvalid(string filename)
    {
        if (!File.Exists(filename)) return true;
        var fileInfo = new FileInfo(filename);
        return fileInfo.Length == 0;
    }
}