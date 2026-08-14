using OsuScoreStats.DbService.Entities;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.OsuEntityToDtoService;

public interface IOsuEntityToDtoService
{
    Score ScoreEntityToDto(APIScore apiScore);
    User UserEntityToDto(APIUser user);
    Beatmap BeatmapEntityToDto(APIBeatmap beatmap);
    Beatmapset BeatmapsetEntityToDto(APIBeatmapset apiBeatmapset);
    Country CountryEntityToDto(APICountry apiCountry);
}