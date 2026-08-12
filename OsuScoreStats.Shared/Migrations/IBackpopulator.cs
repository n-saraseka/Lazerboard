namespace OsuScoreStats.Shared.Migrations;

public interface IBackpopulator
{
    Task BackpopulateAsync(CancellationToken token);
}