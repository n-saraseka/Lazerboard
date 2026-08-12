using OsuScoreStats.Shared.DbService.Entities;

namespace OsuScoreStats.Api;

public static class FilterUtils
{
    /// <summary>
    /// Filter a <see cref="IQueryable{Score}"/> based on the given data
    /// </summary>
    /// <param name="query">A <see cref="IQueryable{Score}"/></param>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <returns>A filtered <see cref="IQueryable{Score}"/></returns>
    public static IQueryable<Score> FilterScoreQuery(IQueryable<Score> query,
        ScoreQueryCommand command)
    {
        if (command.Modes.Length > 0)
        {
            query = query.Where(s => command.Modes.Contains(s.Mode));
        }
        
        if (command.DateRange.Count > 0)
        {
            if (command.DateRange[0] != null) query = query.Where(s => DateOnly.FromDateTime(s.Date) >= command.DateRange[0]);
            if (command.DateRange[1] != null) query = query.Where(s => DateOnly.FromDateTime(s.Date) <= command.DateRange[1]);
        }
        
        if (command.RankRange.Count > 0)
        {
            if (command.RankRange[0] != null) query = query.Where(s => s.Rank >= command.RankRange[0]);
            if (command.RankRange[1] != null) query = query.Where(s => s.Rank <= command.RankRange[1]);
        }

        if (command.PpRange.Count > 0)
        {
            if (command.PpRange[0] != null) query = query.Where(s => s.PP >= command.PpRange[0]);
            if (command.PpRange[1] != null) query = query.Where(s => s.PP <= command.PpRange[1]);
        }

        if (command.AccuracyRange.Count > 0)
        {
            if (command.AccuracyRange[0] != null) query = query.Where(s => s.Accuracy >= command.AccuracyRange[0] / 100.0f);
            if (command.AccuracyRange[1] != null) query = query.Where(s => s.Accuracy <= command.AccuracyRange[1] / 100.0f);
        }

        if (command.SpeedRange.Count > 0)
        {
            // Don't care for Wind Up and Wind Down scores if we filter by minimum / maximum rate
            // SpeedChange is null only if one of the following command.Mods is active: Wind Up, Wind Down, Adaptive Speed
            if (command.SpeedRange[0] != null || command.SpeedRange[1] != null) query = query.Where(s => s.SpeedChange != null);
            if (command.SpeedRange[0] != null) query = query.Where(s => s.SpeedChange >= command.SpeedRange[0]);
            if (command.SpeedRange[1] != null) query = query.Where(s => s.SpeedChange <= command.SpeedRange[1]);
        }

        if (command.StarRange.Count > 0)
        {
            if (command.StarRange[0] != null) query = query.Where(s => s.Beatmap.Difficulty >= command.StarRange[0]);
            if (command.StarRange[1] != null) query = query.Where(s => s.Beatmap.Difficulty <= command.StarRange[1]);
        }

        if (command.LenientMode != null)
        {
            switch (command.LenientMode)
            {
                case true when command.IncludeMods.Length != 0:
                    query = query.Where(s => command.IncludeMods.All(a => s.ModAcronyms.Contains(a)));
                    break;
                case false:
                    query = command.IncludeMods.Length == 0 
                        ? query.Where(s => s.ModAcronyms.Count == 0)
                        : query.Where(s => command.IncludeMods.All(a => s.ModAcronyms.Contains(a)) && s.ModAcronyms.Count == command.IncludeMods.Length);
                    break;
            }
        }

        if (command.ExcludeMods.Length > 0) query = query.Where(s => s.ModAcronyms.All(a => !command.ExcludeMods.Contains(a)));
        
        if (command.CountryCode != null) query = query.Where(s => s.User.CountryCode == command.CountryCode);

        if (command.SortBy != null && command.IsDescending != null)
        {
            var castedDesc = (bool)command.IsDescending;
            switch (command.SortBy)
            {
                // Smaller rank = better, so we use OrderBy instead of OrderByDescending if the sort order chosen by user is descending
                case "rank":
                    query = castedDesc ? query.OrderBy(s => s.Rank) : query.OrderByDescending(s => s.Rank);
                    break;
                case "accuracy":
                    query = castedDesc ? query.OrderByDescending(s => s.Accuracy) : query.OrderBy(s => s.Accuracy);
                    break;
                case "totalScore":
                    query = castedDesc ? query.OrderByDescending(s => s.TotalScore) : query.OrderBy(s => s.TotalScore);
                    break;
                case "classicTotalScore":
                    query = castedDesc ? query.OrderByDescending(s => s.ClassicTotalScore) : query.OrderBy(s => s.ClassicTotalScore);
                    break;
                case "date":
                    query = castedDesc ? query.OrderByDescending(s => s.Date) : query.OrderBy(s => s.Date);
                    break;
                case "combo":
                    query = castedDesc ? query.OrderByDescending(s => s.Combo) : query.OrderBy(s => s.Combo);
                    break;
                default:
                    query = castedDesc ? query.OrderByDescending(s => s.PP) : query.OrderBy(s => s.PP);
                    break;
            }
        }

        return query;
    }
}