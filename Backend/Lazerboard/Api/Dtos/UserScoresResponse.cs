namespace Lazerboard.Api.Dtos;

public class UserScoresResponse : ScoresResponse
{
    public int FilteredCount { get; set; }
}