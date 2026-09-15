using Lazerboard.Data.Database.Entities;
using Lazerboard.Data.Database.Entities.Enums;
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;

namespace Lazerboard.ScoreFetcher.OsuEntityToDtoService;

public interface IOsuEntityToDtoService
{
    Score ScoreEntityToDto(APIScore score, ScoreSource source, Mode beatmapMode);
    User UserEntityToDto(APIUser user);
    Beatmap BeatmapEntityToDto(APIBeatmap beatmap);
    Beatmapset BeatmapsetEntityToDto(APIBeatmapset apiBeatmapset);
    Country CountryEntityToDto(APICountry apiCountry);
}