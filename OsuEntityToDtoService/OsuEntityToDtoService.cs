using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.OsuEntityToDtoService;

public class OsuEntityToDtoService : IOsuEntityToDtoService
{
    public Score ScoreEntityToDto(OsuApi.OsuApiEntities.Score score)
    {
        var dto = new Score
        {
            Id = score.Id,
            Date = score.Date,
            Combo = score.Combo,
            TotalScore = score.TotalScore,
            LegacyTotalScore = score.LegacyTotalScore,
            ClassicTotalScore = score.ClassicTotalScore,
            Misses = score.Statistics.CountMiss,
            BeatmapId = score.BeatmapId,
            Accuracy = score.Accuracy,
            PP = score.PP,
            Grade = score.Grade,
            UserId = score.UserId
        };

        foreach (var mod in score.Mods)
        {
            var acronym = mod.Acronym;
            if (mod.Settings.TryGetValue("speed_change", out var value)) acronym += $"({value}x)";
            dto.ModAcronyms.Add(acronym);
        }

        return dto;
    }

    public User UserEntityToDto(OsuApi.OsuApiEntities.User user) => new User
    {
        Id = user.Id,
        Username = user.Username,
        CountryCode = user.CountryCode
    };

    public Beatmap BeatmapEntityToDto(OsuApi.OsuApiEntities.APIBeatmap beatmap) => new Beatmap
    {
        Id = beatmap.Id,
        BeatmapsetId = beatmap.BeatmapsetId,
        CircleSize = beatmap.CircleSize,
        ApproachRate = beatmap.ApproachRate,
        OverallDifficulty = beatmap.OverallDifficulty,
        DrainLength = beatmap.DrainLength,
        Difficulty = beatmap.Difficulty,
        Status = beatmap.Status,
        BPM = beatmap.BPM,
        Mode = beatmap.Mode,
        DifficultyName = beatmap.DifficultyName
    };

    public Beatmapset BeatmapsetEntityToDto(OsuApi.OsuApiEntities.Beatmapset beatmapset) => new Beatmapset
    {
        Id = beatmapset.Id,
        Artist = beatmapset.Artist,
        Title = beatmapset.Title,
        PreviewUrl = beatmapset.PreviewUrl
    };
    
    public Country CountryEntityToDto(OsuApi.OsuApiEntities.Country country) => new Country
    {
        Id = country.Code,
        Name = country.Name
    };
}