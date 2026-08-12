using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Shared.OsuEntityToDtoService;

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

        var modAcronyms = score.Mods.Select(m => m.Acronym).ToList();

        foreach (var mod in score.Mods)
        {
            var acronym = mod.Acronym;
            if (mod.Settings.TryGetValue("speed_change", out var value))
            {
                if (value is long change)
                    dto.SpeedChange = change;
                else if (value is double changeDouble)
                    // Having to do that because of floating point weirdness.
                    dto.SpeedChange = Math.Floor(changeDouble * 100) / 100;
            }
            
            dto.ModAcronyms.Add(acronym);
        }
        
        if (dto.SpeedChange == null)
        {
            if (modAcronyms.Any(a => a == "DT" || a == "NC"))
                dto.SpeedChange = 1.5;
            if (modAcronyms.Any(a => a == "HT" || a == "DC"))
                dto.SpeedChange = 0.75;
            dto.SpeedChange ??= modAcronyms.Any(a => a == "WD" || a == "WU" || a == "AS") ? null : 1;
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