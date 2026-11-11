using Lazerboard.Data.Database.Entities.Enums;

namespace Lazerboard.Data.Database.Entities;

public class UserCooldown : IEntity<int>
{
    public int Id { get; set; }
    public User? User { get; set; }
    public int UserId { get; set; }
    public CooldownReason CooldownReason { get; set; }
    public DateTimeOffset StartedOn { get; set; }
    public DateTimeOffset ExpiresOn { get; set; }
}