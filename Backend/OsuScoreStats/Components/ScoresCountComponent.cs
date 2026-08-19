using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OsuScoreStats.Data.Database.Repositories.Interfaces;

namespace OsuScoreStats.Components;

public class ScoresCountComponent(IScoreRepository scoreRepository) : ViewComponent
{
    public async Task<string> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var count = await scoreRepository.GetAll().CountAsync(cancellationToken);
        return count.ToString();
    }
}