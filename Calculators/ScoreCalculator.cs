using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using OsuScoreStats.OsuApi;
using System.Reflection;
using osu.Game.IO;
using OsuScoreStats.OsuApi.Enums;
using OsuScoreStats.OsuApi.OsuApiEntities;

namespace OsuScoreStats.Calculators;

public class ScoreCalculator(OsuApiService osuApiService, IConfiguration config) : ICalculator
{
    public async Task<float> CalculateAsync(APIScore apiScore, CancellationToken ct)
    {
        // preparing necessary data
        var ruleset = GetRulesetFromScore(apiScore);
        var beatmap = new Beatmap();
        try
        {
            beatmap = await GetBeatmapFileAsync(apiScore.BeatmapId, ct);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Couldn't get the beatmap for score {apiScore.Id}. Beatmap ID: {apiScore.BeatmapId}.");
            Console.WriteLine($"Failed with the following exception: {ex.Message}");
            return 0;
        }
        var scoreInfo = GetScoreInfo(apiScore, beatmap, ruleset);
        var flatWorkingBeatmap = new FlatWorkingBeatmap(beatmap);

        // diffcalc
        var difficultyAttributes = ruleset.CreateDifficultyCalculator(flatWorkingBeatmap).Calculate(scoreInfo.Mods);
        var performanceCalculator = ruleset.CreatePerformanceCalculator();
        var performanceAttributes = await performanceCalculator.CalculateAsync(scoreInfo, difficultyAttributes, ct);

        return (float)performanceAttributes.Total;
    }
    
    /// <summary>
    /// Prepare ScoreInfo object for use in calculating difficulty and performance attributes
    /// </summary>
    /// <param name="apiScore">Score data from the API</param>
    /// <param name="beatmap">Beatmap data for this score</param>
    /// <param name="ruleset">This score's Ruleset</param>
    /// <returns>The populated ScoreInfo</returns>
    private ScoreInfo GetScoreInfo(APIScore apiScore, IBeatmap beatmap, Ruleset ruleset)
    {
        var scoreStatistics = ScoreStatisticsToDict(apiScore.Statistics);
        var maximumStatistics = ScoreStatisticsToDict(apiScore.MaximumStatistics);

        var soloScoreInfo = new SoloScoreInfo
        {
            BeatmapID = apiScore.BeatmapId,
            RulesetID = (int)apiScore.Mode,
            TotalScore = apiScore.TotalScore,
            LegacyTotalScore = apiScore.LegacyTotalScore,
            LegacyScoreId = apiScore.LegacyScoreId,
            Accuracy = apiScore.Accuracy,
            UserID = apiScore.UserId,
            MaxCombo = apiScore.Combo,
            Rank = (ScoreRank)apiScore.Grade,
            EndedAt = apiScore.Date,
            Mods = apiScore.Mods,
            Statistics = scoreStatistics,
            MaximumStatistics = maximumStatistics
        };

        var mods = new List<Mod>();
        foreach (APIMod apiMod in apiScore.Mods)
        {
            var mod = apiMod.ToMod(ruleset);
            mods.Add(mod);
        }
        var modsArray = mods.ToArray();

        return soloScoreInfo.ToScoreInfo(modsArray, beatmap.BeatmapInfo);
    }
    
    /// <summary>
    /// Parses the Ruleset from given API Score data
    /// </summary>
    /// <param name="apiScore">Score object to parse the ruleset from</param>
    /// <returns>Corresponding Ruleset object</returns>
    private Ruleset GetRulesetFromScore(APIScore apiScore) {
        switch (apiScore.Mode)
        {
            case Mode.Osu:
                    return new OsuRuleset();
            case Mode.Taiko:
                    return new TaikoRuleset();
            case Mode.Fruits:
                    return new CatchRuleset();
            default:
                    return new ManiaRuleset();
        }
    }
    
    /// <summary>
    /// Creates a dictionary of statistics for each HitResult from API Statistics data
    /// </summary>
    /// <param name="stats">Hit statistics</param>
    /// <returns>Populated dictionary of statistics for each HitResult</returns>
    private Dictionary<HitResult, int> ScoreStatisticsToDict(Statistics stats)
    {
        Dictionary<HitResult, int> scoreStatistics = new Dictionary<HitResult, int>();

        foreach (var property in typeof(Statistics).GetProperties())
        {
            var hitResultAttribute = property.GetCustomAttribute<HitResultAttribute>();
            if (hitResultAttribute != null)
            {
                int? value = (int?)property.GetValue(stats);
                if (value != null)
                {
                    scoreStatistics[hitResultAttribute.HitResult] = (int)value;
                }
            }
        }
        return scoreStatistics;
    }

    private async Task<Beatmap> GetBeatmapFileAsync(int beatmapId, CancellationToken ct)
    {
        var mapPath = $"{config["CacheFolder"]}/{beatmapId}.osu";
        if (!File.Exists(mapPath))
        {
            await osuApiService.DownloadBeatmapAsync(beatmapId, ct);
            await Task.Delay(TimeSpan.FromSeconds(1), ct);
        }
        
        await using var stream = File.OpenRead(mapPath);
        using var reader = new LineBufferedReader(stream);
        return osu.Game.Beatmaps.Formats.Decoder.GetDecoder<Beatmap>(reader).Decode(reader);
    }
}