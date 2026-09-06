using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Repositories.Interfaces;

namespace Lazerboard.Data.Database.Repositories;

public class PpBlacklistRepository(ScoreDataContext db) : BaseRepository<PpBlacklistBeatmap, int>(db), IPpBlacklistRepository
{
    // Only exists to keep things the same as other repositories for now.
}