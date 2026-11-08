namespace Lazerboard.Data.OsuEntities.OsuApiEntities;

public class TokenCommand
{
    public string? Code { get; set; }
    public string? RedirectUri { get; set; }
    public string? RefreshToken { get; set; }
}