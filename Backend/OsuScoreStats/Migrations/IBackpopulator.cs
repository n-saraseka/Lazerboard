namespace OsuScoreStats.Migrations;

public interface IBackpopulator
{
    Task BackpopulateAsync(CancellationToken token);
}