namespace Lazerboard.Data.Database.Entities;

public interface IEntity<TKey>
{
    TKey Id { get; set; }
}