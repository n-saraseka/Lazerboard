using Lazerboard.Api.Dtos;
using Lazerboard.Data.Database.Entities;

namespace Lazerboard.Api;

public static class DbToApiObjectUtils
{
    public static ApiUser DbUserToApiUser(User user) => new()
    {
        Id = user.Id,
        Username = user.Username,
        CountryCode = user.CountryCode,
        Country = user.Country,
        IsRestricted = user.IsRestricted
    };

    public static ApiBeatmapset DbBeatmapsetToApiBeatmapset(Beatmapset beatmapset) => new()
    {
        Id = beatmapset.Id,
        Artist = beatmapset.Artist,
        Creator = beatmapset.Creator,
        RankedDate = beatmapset.RankedDate,
        Title = beatmapset.Title,
        User = beatmapset.User == null ? null : DbUserToApiUser(beatmapset.User),
        UserId = beatmapset.UserId
    };

    public static ApiBeatmap DbBeatmapToApiBeatmap(Beatmap beatmap) => new() {
        Id = beatmap.Id,
        BeatmapsetId = beatmap.Id,
        Beatmapset = beatmap.Beatmapset == null ? null : DbBeatmapsetToApiBeatmapset(beatmap.Beatmapset),
        Difficulty = beatmap.Difficulty,
        DifficultyName = beatmap.DifficultyName,
        BPM = beatmap.BPM,
        ApproachRate = beatmap.ApproachRate,
        OverallDifficulty = beatmap.OverallDifficulty,
        CircleSize = beatmap.CircleSize,
        Health = beatmap.Health,
        DrainLength = beatmap.DrainLength,
        Mode = beatmap.Mode,
        Status = beatmap.Status
    };

    public static ApiScore DbScoreToApiScore(Score score) => new()
    {
        Id = score.Id,
        Date = score.Date,
        Mode = score.Mode,
        Beatmap = score.Beatmap == null ? null : DbBeatmapToApiBeatmap(score.Beatmap),
        BeatmapId = score.BeatmapId,
        User = score.User == null ? null : DbUserToApiUser(score.User),
        UserId = score.UserId,
        Grade = score.Grade,
        ModAcronyms = score.ModAcronyms,
        SpeedChange = score.SpeedChange,
        Accuracy = score.Accuracy,
        Combo = score.Combo,
        TotalScore = score.TotalScore,
        ClassicTotalScore = score.ClassicTotalScore,
        LegacyTotalScore = score.LegacyTotalScore,
        PP = score.PP,
        Rank = score.Rank,
        IsConvert = score.IsConvert,
        IsLazerScore = score.IsLazerScore,
        IsPerfectCombo = score.IsPerfectCombo,
        Statistics = score.Statistics
    };
}