using Lazerboard.Api.Dtos;
using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.Api.Utils;

public static class FilterUtils
{
    private static readonly BeatmapStatus[] RankedStatuses = [BeatmapStatus.Ranked, BeatmapStatus.Approved];
    /// <summary>
    /// Filter a <see cref="IQueryable{Score}"/> based on the given data
    /// </summary>
    /// <param name="query">A <see cref="IQueryable{Score}"/></param>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <returns>A filtered <see cref="IQueryable{Score}"/></returns>
    public static IQueryable<Score> FilterScoreQuery(IQueryable<Score> query,
        ScoreQueryCommand command)
    {
        if (command.DateRange[0] != null)
        {
            var minDate = DateTime
                .SpecifyKind(command.DateRange[0]!.Value.ToDateTime(new TimeOnly(0, 0)), DateTimeKind.Utc);
            query = query.Where(s => s.Date >= minDate);
        }
        if (command.DateRange[1] != null && command.DateRange[1] < DateOnly.FromDateTime(DateTime.UtcNow))
        {
            var maxDate = DateTime
                .SpecifyKind(command.DateRange[1]!.Value.ToDateTime(new TimeOnly(0, 0)), DateTimeKind.Utc).AddDays(1);
            query = query.Where(s => s.Date < maxDate);
        }
        
        // Don't care for Wind Up and Wind Down scores if we filter by minimum / maximum rate
        // SpeedChange is null only if one of the following command.Mods is active: Wind Up, Wind Down, Adaptive Speed
        if (command.SpeedRange[0] != null || command.SpeedRange[1] != null) query = query.Where(s => s.SpeedChange != null);
        if (command.SpeedRange[0] != null && command.SpeedRange[0] > 0.5) query = query.Where(s => s.SpeedChange >= command.SpeedRange[0]);
        if (command.SpeedRange[1] != null && command.SpeedRange[1] < 2) query = query.Where(s => s.SpeedChange <= command.SpeedRange[1]);
        
        if (command.PpRange[0] != null && command.PpRange[0] > 0) query = query.Where(s => s.PP >= command.PpRange[0]);
        if (command.PpRange[1] != null) query = query.Where(s => s.PP <= command.PpRange[1]);
        
        if (command.RankRange[0] != null && command.RankRange[0] > 1) query = query.Where(s => s.Rank >= command.RankRange[0]);
        if (command.RankRange[1] != null && command.RankRange[1] < 100) query = query.Where(s => s.Rank <= command.RankRange[1]);
        
        if (command.AccuracyRange[0] != null) {
            var acc = command.AccuracyRange[0] / 100.0f;
            query = query.Where(s => s.Accuracy >= acc);
        }
        if (command.AccuracyRange[1] != null && command.AccuracyRange[1] < 100) {
            var acc = command.AccuracyRange[1] / 100.0f;
            query = query.Where(s => s.Accuracy <= acc);
        }
        
        if (command.StarRange[0] != null && command.SpeedRange[1] > 0) query = query.Where(s => s.Beatmap!.Difficulty >= command.StarRange[0]);
        if (command.StarRange[1] != null) query = query.Where(s => s.Beatmap!.Difficulty <= command.StarRange[1]);
        
        if (command.BeatmapStatus != null)
        {
            query = command.BeatmapStatus == BeatmapStatus.Ranked
                ? query.Where(s => RankedStatuses.Contains(s.Beatmap!.Status))
                : query.Where(s => s.Beatmap!.Status == command.BeatmapStatus);
        }
        
        if (command.Modes != null && !(command.Modes.Length == 4 && command.Modes.Distinct().Count() == command.Modes.Length))
        {
            var modes = command.Modes.Distinct().ToList();
            query = query.Where(s => modes.Contains(s.Mode));
        }
        
        if (command.ExcludeConverts != null)
        {
            query = command.ExcludeConverts.Value ? query.Where(s => s.IsConvert == false) : query;
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
        
        if (command.CountryCode != null) query = query.Where(s => s.User!.CountryCode == command.CountryCode);

        if (command.SortBy != null && command.IsDescending != null)
        {
            var isDescending = command.IsDescending.Value;
            switch (command.SortBy)
            {
                // Smaller rank = better, so we use OrderBy instead of OrderByDescending if the sort order chosen by user is descending
                case "rank":
                    query = isDescending 
                        ? query.OrderBy(s => s.Rank).ThenByDescending(s => s.PP)
                        : query.OrderByDescending(s => s.Rank).ThenByDescending(s => s.PP);
                    break;
                case "accuracy":
                    query = isDescending 
                        ? query.OrderByDescending(s => s.Accuracy).ThenByDescending(s => s.PP)
                        : query.OrderBy(s => s.Accuracy).ThenByDescending(s => s.PP);
                    break;
                case "totalScore":
                    query = isDescending 
                        ? query.OrderByDescending(s => s.TotalScore).ThenByDescending(s => s.PP)
                        : query.OrderBy(s => s.TotalScore).ThenByDescending(s => s.PP);
                    break;
                case "classicTotalScore":
                    query = isDescending 
                        ? query.OrderByDescending(s => s.ClassicTotalScore).ThenByDescending(s => s.PP)
                        : query.OrderBy(s => s.ClassicTotalScore).ThenByDescending(s => s.PP);
                    break;
                case "date":
                    query = isDescending 
                        ? query.OrderByDescending(s => s.Date).ThenByDescending(s => s.PP)
                        : query.OrderBy(s => s.Date).ThenByDescending(s => s.PP);
                    break;
                case "combo":
                    query = isDescending 
                        ? query.OrderByDescending(s => s.Combo).ThenByDescending(s => s.PP)  
                        : query.OrderBy(s => s.Combo).ThenByDescending(s => s.PP);
                    break;
                case "stars":
                    query = isDescending
                        ? query.OrderByDescending(s => s.Beatmap!.Difficulty).ThenByDescending(s => s.PP)
                        : query.OrderBy(s => s.Beatmap!.Difficulty).ThenByDescending(s => s.PP);
                    break;
                default:
                    query = isDescending 
                        ? query.OrderByDescending(s => s.PP).ThenByDescending(s => s.Rank) 
                        : query.OrderBy(s => s.PP).ThenByDescending(s => s.Rank);
                    break;
            }
        }

        return query;
    }

    /// <summary>
    /// Clamp range values in a <see cref="ScoreQueryCommand"/>
    /// </summary>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <returns>The updated <see cref="ScoreQueryCommand"/></returns>
    public static ScoreQueryCommand GetCommandWithClampedRanges(ScoreQueryCommand command)
    {
        var currentDate = DateOnly.FromDateTime(DateTime.UtcNow);
        if (command.DateRange[0] != null) command.DateRange[0] = command.DateRange[0] > currentDate ? currentDate : command.DateRange[0];
        if (command.DateRange[1] != null) command.DateRange[1] = command.DateRange[1] >= currentDate ? null : command.DateRange[1];

        if (command.RankRange[0] != null) command.RankRange[0] = command.RankRange[0] <= 1 ? null : command.RankRange[0];
        if (command.RankRange[1] != null) command.RankRange[1] = command.RankRange[0] >= 100 ? null : command.RankRange[1];
        
        if (command.PpRange[0] != null) command.PpRange[0] = command.PpRange[0] <= 0 ? null : command.PpRange[0];
        if (command.PpRange[1] != null) command.PpRange[1] = Math.Max(command.PpRange[1]!.Value, 0);

        if (command.SpeedRange[0] != null)
            command.SpeedRange[0] = command.SpeedRange[0] <= 0.5 ? null : command.SpeedRange[0];
        if (command.SpeedRange[1] != null) command.SpeedRange[1] = command.SpeedRange[1] >= 2 ? null : command.SpeedRange[1];

        if (command.StarRange[0] != null) command.StarRange[0] = command.StarRange[0] <= 0 ? null : command.StarRange[0];
        if (command.StarRange[1] != null) command.StarRange[1] = Math.Max(command.StarRange[1]!.Value, 0);
        
        return command;
    }
}
