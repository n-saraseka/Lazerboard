using OsuScoreStats.DbService.Entities;
using OsuScoreStats.OsuApi.Enums;

namespace OsuScoreStats.Api;

public static class FilterUtils
{
    /// <summary>
    /// Filter a <see cref="IQueryable{Score}"/> based on the given data
    /// </summary>
    /// <param name="query">A <see cref="IQueryable{Score}"/></param>
    /// <param name="modes">An array with allowed <see cref="Mode"/>s</param>
    /// <param name="dateRange">A <see cref="List{DateOnly}"/> with two Date values</param>
    /// <param name="rankRange">A <see cref="List{int}"/> with two <see cref="Score"/>.Rank values</param>
    /// <param name="ppRange">A <see cref="List{int}"/> with two <see cref="Score"/>.PP values</param>
    /// <param name="accRange">A <see cref="List{double}"/> with two <see cref="Score"/>.Accuracy values</param>
    /// <param name="speedRange">A <see cref="List{double}"/> with two <see cref="Score"/>.SpeedChange values</param>
    /// <param name="starRange">A <see cref="List{double}"/> with two <see cref="Beatmap"/>.Difficulty values</param>
    /// <param name="mods">An array of mod acronyms to filter scores</param>
    /// <param name="lenientMode">Whether to allow other mods than those listed in <paramref name="mods"/> or not</param>
    /// <param name="countryCode">The <see cref="Country"/> code to filter scores by</param>
    /// <param name="sort">Name of the field to sort scores by</param>
    /// <param name="isDesc">Whether sort is descending or not</param>
    /// <remarks>The {x}Range parameters refer to <see cref="List{T}"/>s of two values, the minimum and the maximum.
    /// When provided, it filters the query to look up scores between the two values </remarks>
    /// <returns></returns>
    public static IQueryable<Score> FilterScoreQuery(IQueryable<Score> query,
        Mode[] modes,
        List<DateOnly?> dateRange,
        List<int?> rankRange,
        List<int?> ppRange,
        List<double?> accRange,
        List<double?> speedRange,
        List<double?> starRange,
        string[] mods,
        bool? lenientMode,
        string? countryCode,
        string? sort,
        bool? isDesc)
    {
        if (modes.Length > 0)
        {
            query = query.Where(s => modes.Contains(s.Mode));
        }
        
        if (dateRange.Count > 0)
        {
            if (dateRange[0] != null) query = query.Where(s => DateOnly.FromDateTime(s.Date) >= dateRange[0]);
            if (dateRange[1] != null) query = query.Where(s => DateOnly.FromDateTime(s.Date) <= dateRange[1]);
        }
        
        if (rankRange.Count > 0)
        {
            if (rankRange[0] != null) query = query.Where(s => s.Rank >= rankRange[0]);
            if (rankRange[1] != null) query = query.Where(s => s.Rank <= rankRange[1]);
        }

        if (ppRange.Count > 0)
        {
            if (ppRange[0] != null) query = query.Where(s => s.PP >= ppRange[0]);
            if (ppRange[1] != null) query = query.Where(s => s.PP <= ppRange[1]);
        }

        if (accRange.Count > 0)
        {
            if (accRange[0] != null) query = query.Where(s => s.Accuracy >= accRange[0] / 100.0f);
            if (accRange[1] != null) query = query.Where(s => s.Accuracy <= accRange[1] / 100.0f);
        }

        if (speedRange.Count > 0)
        {
            // Don't care for Wind Up and Wind Down scores if we filter by minimum / maximum rate
            // SpeedChange is null only if one of the following mods is active: Wind Up, Wind Down, Adaptive Speed
            if (speedRange[0] != null || speedRange[1] != null) query = query.Where(s => s.SpeedChange != null);
            if (speedRange[0] != null) query = query.Where(s => s.SpeedChange >= speedRange[0]);
            if (speedRange[1] != null) query = query.Where(s => s.SpeedChange <= speedRange[1]);
        }

        if (starRange.Count > 0)
        {
            if (starRange[0] != null) query = query.Where(s => s.Beatmap.Difficulty >= starRange[0]);
            if (starRange[1] != null) query = query.Where(s => s.Beatmap.Difficulty <= starRange[1]);
        }

        if (lenientMode != null)
        {
            switch (lenientMode)
            {
                case true when mods.Length != 0:
                    query = query.Where(s => mods.All(a => s.ModAcronyms.Contains(a)));
                    break;
                case false:
                    query = mods.Length == 0 
                        ? query.Where(s => s.ModAcronyms.Count == 0)
                        : query.Where(s => mods.All(a => s.ModAcronyms.Contains(a)) && s.ModAcronyms.Count == mods.Length);
                    break;
            }
        }
        
        if (countryCode != null) query = query.Where(s => s.User.CountryCode == countryCode);

        if (sort != null && isDesc != null)
        {
            var castedDesc = (bool)isDesc;
            switch (sort)
            {
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
                default:
                    query = castedDesc ? query.OrderByDescending(s => s.PP) : query.OrderBy(s => s.PP);
                    break;
            }
        }

        return query;
    }
}