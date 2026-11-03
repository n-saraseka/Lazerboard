using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Api;

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
        if (!(command.Modes.Length == 4 && command.Modes.Distinct().Count() == command.Modes.Length))
        {
            var modes = command.Modes.Distinct().ToList();
            query = query.Where(s => modes.Contains(s.Mode));
        }

        if (command.BeatmapStatus != null)
        {
            query = query.Where(s => s.Beatmap.Status == command.BeatmapStatus);
        }

        if (command.ExcludeConverts != null)
        {
            query = command.ExcludeConverts.Value ? query.Where(s => s.IsConvert == false) : query;
        }
        
        if (command.DateRange[0] != null)
        {
            var minDate = DateTime
                .SpecifyKind(command.DateRange[0]!.Value.ToDateTime(new TimeOnly(0, 0)), DateTimeKind.Utc);
            query = query.Where(s => s.Date >= minDate);
        }
        if (command.DateRange[1] != null)
        {
            var maxDate = DateTime
                .SpecifyKind(command.DateRange[1]!.Value.ToDateTime(new TimeOnly(0, 0)), DateTimeKind.Utc).AddDays(1);
            query = query.Where(s => s.Date < maxDate);
        }
        
        if (command.RankRange[0] != null) query = query.Where(s => s.Rank >= command.RankRange[0]);
        if (command.RankRange[1] != null) query = query.Where(s => s.Rank <= command.RankRange[1]);

        if (command.PpRange[0] != null) query = query.Where(s => s.PP >= command.PpRange[0]);
        if (command.PpRange[1] != null) query = query.Where(s => s.PP <= command.PpRange[1]);

        if (command.AccuracyRange[0] != null) {
            var acc = command.AccuracyRange[0] / 100.0f;
            query = query.Where(s => s.Accuracy >= acc);
        }
        if (command.AccuracyRange[1] != null) {
            var acc = command.AccuracyRange[1] / 100.0f;
            query = query.Where(s => s.Accuracy <= acc);
        }

        // Don't care for Wind Up and Wind Down scores if we filter by minimum / maximum rate
        // SpeedChange is null only if one of the following command.Mods is active: Wind Up, Wind Down, Adaptive Speed
        if (command.SpeedRange[0] != null || command.SpeedRange[1] != null) query = query.Where(s => s.SpeedChange != null);
        if (command.SpeedRange[0] != null) query = query.Where(s => s.SpeedChange >= command.SpeedRange[0]);
        if (command.SpeedRange[1] != null) query = query.Where(s => s.SpeedChange <= command.SpeedRange[1]);

        if (command.StarRange[0] != null) query = query.Where(s => s.Beatmap.Difficulty >= command.StarRange[0]);
        if (command.StarRange[1] != null) query = query.Where(s => s.Beatmap.Difficulty <= command.StarRange[1]);

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
                case "stars":
                    query = castedDesc ? query.OrderByDescending(s => s.Beatmap.Difficulty) : query.OrderBy(s => s.Beatmap.Difficulty);
                    break;
                default:
                    query = castedDesc ? query.OrderByDescending(s => s.PP) : query.OrderBy(s => s.PP);
                    break;
            }
        }

        return query;
    }

    /// <summary>
    /// Get filter string from a <see cref="ScoreQueryCommand"/>
    /// </summary>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <returns>The filter string</returns>
    public static string GetFilterText(ScoreQueryCommand command)
    {
        var filters = new List<string>();
        
        if (!(command.Modes.Length == 4 && command.Modes.Distinct().Count() == command.Modes.Length))
        {
            var modes = command.Modes.Select(m => $"'{m.ToString()}'".ToLower()).Distinct().ToList();
            filters.Add($"mode = ANY({string.Join(",", modes)})");
        }
        
        if (command.BeatmapStatus != null)
        {
            filters.Add($"b.status = '{command.BeatmapStatus.Value.ToString().ToLower()}'");
        }

        if (command.ExcludeConverts != null && command.ExcludeConverts.Value)
        {
            filters.Add($"is_convert = FALSE");
        }
        
        if (command.DateRange[0] != null)
        {
            var minDate = DateTime
                .SpecifyKind(command.DateRange[0]!.Value.ToDateTime(new TimeOnly(0, 0)), DateTimeKind.Utc);
            filters.Add($"date >= {minDate}");
        }
        if (command.DateRange[1] != null)
        {
            var maxDate = DateTime
                .SpecifyKind(command.DateRange[1]!.Value.ToDateTime(new TimeOnly(0, 0)), DateTimeKind.Utc).AddDays(1);
            filters.Add($"date < {maxDate}");
        }
        
        if (command.RankRange[0] != null) filters.Add($"rank >= {command.RankRange[0]}");
        if (command.RankRange[1] != null) filters.Add($"rank <= {command.RankRange[1]}");

        if (command.PpRange[0] != null) filters.Add($"pp >= {command.PpRange[0]}");
        if (command.PpRange[1] != null) filters.Add($"pp <= {command.PpRange[0]}");

        if (command.AccuracyRange[0] != null) {
            var acc = command.AccuracyRange[0] / 100.0f;
            filters.Add($"accuracy >= {acc}");
        }
        if (command.AccuracyRange[1] != null) {
            var acc = command.AccuracyRange[1] / 100.0f;
            filters.Add($"accuracy <= {acc}");
        }

        // Don't care for Wind Up and Wind Down scores if we filter by minimum / maximum rate
        // SpeedChange is null only if one of the following command.Mods is active: Wind Up, Wind Down, Adaptive Speed
        if (command.SpeedRange[0] != null || command.SpeedRange[1] != null) filters.Add("speed_change IS NOT NULL");
        if (command.SpeedRange[0] != null) filters.Add($"speed_change >= {command.SpeedRange[0]}");
        if (command.SpeedRange[1] != null) filters.Add($"speed_change <= {command.SpeedRange[1]}");

        if (command.StarRange[0] != null) filters.Add($"b.difficulty >= {command.StarRange[0]}");
        if (command.StarRange[1] != null) filters.Add($"b.difficulty <= {command.StarRange[1]}");

        if (command.LenientMode != null)
        {
            switch (command.LenientMode)
            {
                case true when command.IncludeMods.Length != 0:
                    var mods = command.IncludeMods.Select(m => $"'{m}'").Distinct().ToList();
                    filters.Add($"mods <@ ARRAY[{string.Join(",", mods)}]");
                    break;
                case false:
                    if (command.IncludeMods.Length > 0)
                    {
                        mods = command.IncludeMods.Select(m => $"'{m}'").Distinct().ToList();
                        filters.Add($"mods @> ARRAY[{string.Join(",", mods)}]");
                        filters.Add($"mods <@ ARRAY[{string.Join(",", mods)}]");
                    }
                    else
                    {
                        filters.Add("cardinality(mods) = 0");
                    }
                    break;
            }
        }

        if (command.ExcludeMods.Length > 0)
        {
            var mods = command.ExcludeMods.Select(m => $"'{m}'").Distinct().ToList();
            filters.Add($"mods NOT <@ ARRAY[{string.Join(",", mods)}]");
        }
        
        if (command.CountryCode != null) filters.Add($"u.country_code != {command.CountryCode}");
        
        return string.Join(" AND ",  filters);
    }

    /// <summary>
    /// Get table string from <see cref="ScoreQueryCommand"/>
    /// </summary>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <param name="joinBeatmaps">Whether to add an INNER JOIN on beatmaps anyway</param>
    /// <returns>The string</returns>
    public static string GetTableStringWithJoins(ScoreQueryCommand command, bool joinBeatmaps)
    {
        var tableString = "scores s";
        var joins = new List<string>();
        if (command.BeatmapStatus != null || joinBeatmaps)
        {
            joins.Add("INNER JOIN beatmaps b ON s.beatmap_id = b.id");
        }

        if (command.CountryCode != null)
        {
            joins.Add("INNER JOIN users u ON s.user_id = u.id");
        }

        if (joins.Count == 0) return tableString;
        
        var joinsString = string.Join(" ", joins);
        return $"{tableString} {joinsString}";
    }
}
