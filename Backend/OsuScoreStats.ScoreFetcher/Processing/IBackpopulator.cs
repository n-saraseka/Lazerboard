namespace OsuScoreStats.ScoreFetcher.Processing;

public interface IBackpopulator
{
    Task BackpopulateAsync(CancellationToken token);
}