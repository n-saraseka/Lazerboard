using OsuScoreStats.DbService.Entities;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.OsuEntityToDtoService;

public class OsuEntityToDtoService : IOsuEntityToDtoService
{
    public Score ScoreEntityToDto(APIScore apiScore)
    {
        var dto = new Score
        {
            Id = apiScore.Id,
            Date = apiScore.Date,
            Combo = apiScore.Combo,
            TotalScore = apiScore.TotalScore,
            LegacyTotalScore = apiScore.LegacyTotalScore,
            ClassicTotalScore = apiScore.ClassicTotalScore,
            Misses = apiScore.Statistics.CountMiss,
            BeatmapId = apiScore.BeatmapId,
            Accuracy = apiScore.Accuracy,
            PP = apiScore.PP,
            Grade = apiScore.Grade,
            UserId = apiScore.UserId
        };

        foreach (var mod in apiScore.Mods)
        {
            var acronym = mod.Acronym;
            if (mod.Settings.TryGetValue("speed_change", out var value)) acronym += $"({value}x)";
            dto.ModAcronyms.Add(acronym);
        }

        return dto;
    }

    public User UserEntityToDto(APIUser user) => new User
    {
        Id = user.Id,
        Username = user.Username,
        CountryCode = user.CountryCode
    };

    public Beatmap BeatmapEntityToDto(APIBeatmap beatmap) => new Beatmap
    {
        Id = beatmap.Id,
        BeatmapsetId = beatmap.BeatmapsetId,
        CircleSize = beatmap.CircleSize,
        ApproachRate = beatmap.ApproachRate,
        OverallDifficulty = beatmap.OverallDifficulty,
        Health = beatmap.Health,
        DrainLength = beatmap.DrainLength,
        Difficulty = beatmap.Difficulty,
        Status = beatmap.Status,
        BPM = beatmap.BPM,
        Mode = beatmap.Mode,
        DifficultyName = beatmap.DifficultyName
    };

    public Beatmapset BeatmapsetEntityToDto(APIBeatmapset apiBeatmapset) => new Beatmapset
    {
        Id = apiBeatmapset.Id,
        Artist = apiBeatmapset.Artist,
        Title = apiBeatmapset.Title,
        PreviewUrl = apiBeatmapset.PreviewUrl
    };
    
    public Country CountryEntityToDto(APICountry apiCountry) => new Country
    {
        Id = apiCountry.Code,
        Name = apiCountry.Name
    };
}