using OsuScoreStats.Data.Database.Entities;
using OsuScoreStats.Data.OsuEntities.OsuApiEntities;

namespace OsuScoreStats.ScoreFetcher.OsuEntityToDtoService;

public interface IOsuEntityToDtoService
{
    Score ScoreEntityToDto(APIScore apiScore);
    User UserEntityToDto(APIUser user);
    Beatmap BeatmapEntityToDto(APIBeatmap beatmap);
    Beatmapset BeatmapsetEntityToDto(APIBeatmapset apiBeatmapset);
    Country CountryEntityToDto(APICountry apiCountry);
}