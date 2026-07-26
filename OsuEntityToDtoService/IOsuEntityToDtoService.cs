using OsuScoreStats.DbService.Entities;

namespace OsuScoreStats.OsuEntityToDtoService;

public interface IOsuEntityToDtoService
{
    Score ScoreEntityToDto(OsuApi.OsuApiEntities.Score score);
    User UserEntityToDto(OsuApi.OsuApiEntities.User user);
    Beatmap BeatmapEntityToDto(OsuApi.OsuApiEntities.APIBeatmap beatmap);
    Beatmapset BeatmapsetEntityToDto(OsuApi.OsuApiEntities.Beatmapset beatmapset);
    Country CountryEntityToDto(OsuApi.OsuApiEntities.Country country);
}