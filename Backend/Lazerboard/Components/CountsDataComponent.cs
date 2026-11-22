using Microsoft.AspNetCore.Mvc;
using Lazerboard.Data.Redis.Repositories.Interfaces;

namespace Lazerboard.Components;

public class CountsDataComponent(IScoreCacheRepository scoreCache) : ViewComponent
{
    public async Task<string> InvokeAsync()
    {
        var data = await scoreCache.GetSerializedResponseAsync("counts-data");
        return data ?? "";
    }
}