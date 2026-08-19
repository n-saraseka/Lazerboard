using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Components;

public class ScoresCountComponent(IScoreRepository scoreRepository) : ViewComponent
{
    public async Task<string> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var count = await scoreRepository.GetAll().CountAsync(cancellationToken);
        return count.ToString();
    }
}