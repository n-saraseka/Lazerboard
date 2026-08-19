using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.ScoreFetcher.OsuEntityToDtoService;

public interface IOsuEntityToDtoService
{
    Score ScoreEntityToDto(APIScore apiScore);
    User UserEntityToDto(APIUser user);
    Beatmap BeatmapEntityToDto(APIBeatmap beatmap);
    Beatmapset BeatmapsetEntityToDto(APIBeatmapset apiBeatmapset);
    Country CountryEntityToDto(APICountry apiCountry);
}