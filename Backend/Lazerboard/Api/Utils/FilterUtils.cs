using Lazerboard.Api.Dtos;
using Lazerboard.Data.Database.Entities;
using Npgsql;

namespace Lazerboard.Api.Utils;

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
            query = query.Where(s => s.Beatmap!.Status == command.BeatmapStatus);
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

        if (command.StarRange[0] != null) query = query.Where(s => s.Beatmap!.Difficulty >= command.StarRange[0]);
        if (command.StarRange[1] != null) query = query.Where(s => s.Beatmap!.Difficulty <= command.StarRange[1]);

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
                        ? query.OrderByDescending(s => s.ClassicTotalScore).ThenByDescending(s => s.Id)
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
    /// Get filter string from a <see cref="ScoreQueryCommand"/>
    /// </summary>
    /// <param name="command">The <see cref="ScoreQueryCommand"/></param>
    /// <returns>The corresponding <see cref="ScoreFiltersData"/></returns>
    public static ScoreFiltersData GetFilterText(ScoreQueryCommand command)
    {
        var filters = new List<string>();
        var parameters = new List<object>();
        
        if (!(command.Modes.Length == 4 && command.Modes.Distinct().Count() == command.Modes.Length))
        {
            var modes = command.Modes.Distinct().ToList();
            parameters.Add(new NpgsqlParameter("@modes", modes));
            filters.Add("s.mode = ANY(@modes)");
        }

        if (command.BeatmapStatus != null)
        {
            parameters.Add(new NpgsqlParameter("@status", command.BeatmapStatus.Value.ToString().ToLower()));
            filters.Add("b.status = @status");
        }

        if (command.ExcludeConverts != null && command.ExcludeConverts.Value) filters.Add("is_convert = FALSE");

        if (command.DateRange[0] != null)
        {
            parameters.Add(new NpgsqlParameter("@dateMin", command.DateRange[0]));
            filters.Add("date >= @dateMin");
        }

        if (command.DateRange[1] != null)
        {
            parameters.Add(new NpgsqlParameter("@dateMax", command.DateRange[1]));
            filters.Add("date <= @dateMax");
        }


        if (command.RankRange[0] != null)
        {
            parameters.Add(new NpgsqlParameter("@rankMin", command.RankRange[0]));
            filters.Add("rank >= @rankMin");
        }
        if (command.RankRange[1] != null)
        {
            parameters.Add(new NpgsqlParameter("@rankMax", command.RankRange[1]));
            filters.Add("rank <= @rankMax");
        }

        if (command.PpRange[0] != null)
        {
            parameters.Add(new NpgsqlParameter("@ppMin", command.PpRange[0]));
            filters.Add("pp >= @ppMin");
        }

        if (command.PpRange[1] != null)
        {
            parameters.Add(new NpgsqlParameter("@ppMax", command.PpRange[1]));
            filters.Add("pp <= @ppMax");
        }

        if (command.AccuracyRange[0] != null) {
            var acc = command.AccuracyRange[0] / 100.0f;
            parameters.Add(new NpgsqlParameter("@accMin", acc));
            filters.Add("accuracy >= @accMin");
        }
        if (command.AccuracyRange[1] != null) {
            var acc = command.AccuracyRange[1] / 100.0f;
            parameters.Add(new NpgsqlParameter("@accMax", acc));
            filters.Add("accuracy <= @accMax");
        }

        // Don't care for Wind Up and Wind Down scores if we filter by minimum / maximum rate
        // SpeedChange is null only if one of the following command.Mods is active: Wind Up, Wind Down, Adaptive Speed
        if (command.SpeedRange[0] != null || command.SpeedRange[1] != null) filters.Add("speed_change IS NOT NULL");
        if (command.SpeedRange[0] != null)
        {
            parameters.Add(new NpgsqlParameter("@speedMin", command.SpeedRange[0]));
            filters.Add("speed_change >= @speedMin");
        }

        if (command.SpeedRange[1] != null)
        {
            parameters.Add(new NpgsqlParameter("@speedMax", command.SpeedRange[1]));
            filters.Add("speed_change <= @speedMax");
        }

        if (command.StarRange[0] != null)
        {
            parameters.Add(new NpgsqlParameter("@starMin", command.StarRange[0]));
            filters.Add("b.difficulty >= @starMin");
        }

        if (command.StarRange[1] != null)
        {
            parameters.Add(new NpgsqlParameter("@starMax", command.StarRange[1]));
            filters.Add("b.difficulty <= @starMax");
        }

        if (command.LenientMode != null)
        {
            switch (command.LenientMode)
            {
                case true when command.IncludeMods.Length != 0:
                    var mods = command.IncludeMods.Distinct().ToList();
                    parameters.Add(new NpgsqlParameter("@includeMods", mods));
                    filters.Add("mod_acronyms @> ARRAY[@includeMods]");
                    break;
                case false:
                    if (command.IncludeMods.Length > 0)
                    {
                        mods = command.IncludeMods.Distinct().ToList();
                        parameters.Add(new NpgsqlParameter("@includeMods", mods));
                        filters.Add("mod_acronyms @> ARRAY[@includeMods]");
                        filters.Add("mod_acronyms <@ ARRAY[@includeMods]");
                    }
                    else
                    {
                        filters.Add("cardinality(mod_acronyms) = 0");
                    }
                    break;
            }
        }

        if (command.ExcludeMods.Length > 0)
        {
            var mods = command.ExcludeMods.Distinct().ToList();
            parameters.Add(new NpgsqlParameter("@excludeMods", mods));
            filters.Add("NOT mod_acronyms @> ARRAY[@excludeMods]");
        }

        if (command.CountryCode != null)
        {
            parameters.Add(new NpgsqlParameter("@countryCode", command.CountryCode));
            filters.Add("u.country_code = @countryCode");
        }

        return new ScoreFiltersData
        {
            FilterText = string.Join(" AND ", filters),
            Parameters = parameters.ToArray()
        };
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
