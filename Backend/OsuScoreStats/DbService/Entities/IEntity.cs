namespace OsuScoreStats.DbService.Entities;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
}