namespace Lazerboard.Data.Database.Entities;

public class ScorePendingDeletion : IEntity<int>
{
    public int Id { get; set; }
    public ulong ScoreId { get; set; }
    public Score Score { get; set; }
    public DateTime MarkedAt { get; set; }
}