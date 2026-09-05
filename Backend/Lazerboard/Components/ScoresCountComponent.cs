using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazerboard.Data.Database.Repositories.Interfaces;
using Lazerboard.Data.Redis.Repositories.Interfaces;

namespace Lazerboard.Components;

public class ScoresCountComponent(IScoreCacheRepository scoreCache) : ViewComponent
{
    public async Task<string> InvokeAsync()
    {
        var count = await scoreCache.GetScoresCountAsync();
        return count.HasValue ? count.Value.ToString() : "not available";
    }
}