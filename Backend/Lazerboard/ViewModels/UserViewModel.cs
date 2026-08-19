using Lazerboard.Data.Database.Entities;

namespace Lazerboard.ViewModels;

public class UserViewModel
{
    public User User { get; set; }
    public List<Score> Scores { get; set; }
    public int Count { get; set; }
    public int Pages { get; set; }
}