using OsuScoreStats.DbService.Entities;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.OsuEntityToDtoService;

public class OsuEntityToDtoService : IOsuEntityToDtoService
{
    public Score ScoreEntityToDto(APIScore score)
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
            Mode = score.Mode,
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
        DifficultyName = beatmap.DifficultyName,
    };

    public Beatmapset BeatmapsetEntityToDto(APIBeatmapset beatmapset) => new Beatmapset
    {
        Id = beatmapset.Id,
        Artist = beatmapset.Artist,
        Title = beatmapset.Title,
        Creator = beatmapset.Creator,
        UserId = beatmapset.UserId,
    };
    
    public Country CountryEntityToDto(APICountry country) => new Country
    {
        Id = country.Code,
        Name = country.Name
    };
}