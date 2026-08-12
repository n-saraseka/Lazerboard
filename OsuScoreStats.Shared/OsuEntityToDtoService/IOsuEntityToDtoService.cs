using OsuScoreStats.Shared.DbService.Entities;
using OsuScoreStats.Shared.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Shared.OsuEntityToDtoService;

public interface IOsuEntityToDtoService
{
    Score ScoreEntityToDto(APIScore apiScore);
    User UserEntityToDto(APIUser user);
    Beatmap BeatmapEntityToDto(APIBeatmap beatmap);
    Beatmapset BeatmapsetEntityToDto(APIBeatmapset apiBeatmapset);
    Country CountryEntityToDto(APICountry apiCountry);
}