using System.Reflection;
using Microsoft.Extensions.Logging;
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
using Lazerboard.Data.OsuEntities.Enums;
using Lazerboard.Data.OsuEntities.OsuApiEntities;
using Lazerboard.Data.Redis.Repositories.Interfaces;

namespace Lazerboard.ScoreFetcher.Calculations;

public class ScoreCalculator(ICacheStore cacheStore, 
    ILogger<ScoreCalculator> logger, 
    IScoreCacheRepository scoreCacheRepository) : ICalculator
{
    private static readonly TimeSpan CalculationTimeout = TimeSpan.FromSeconds(30);

    public async Task<float?> CalculateAsync(APIScore apiScore, CancellationToken ct)
    {
        var isCalculatable = await scoreCacheRepository.GetScoreCalculatableAsync(apiScore.BeatmapId, apiScore.Mode);
        if (isCalculatable.HasValue)
        {
            if (!isCalculatable.Value) return null;
        }
        
        var ruleset = GetRulesetFromScore(apiScore);
        Beatmap beatmap;
        try
        {
            beatmap = await cacheStore.GetBeatmapFileAsync(apiScore.BeatmapId, ct);
        }
        catch (Exception ex)
        {
            logger.Log(LogLevel.Error, ex, "Method: ScoreCalculator.CalculateAsync | Score: {score}, Beatmap ID: {beatmapId}", apiScore, apiScore.BeatmapId);
            return null;
        }
        var scoreInfo = GetScoreInfo(apiScore, beatmap, ruleset);
        var flatWorkingBeatmap = new FlatWorkingBeatmap(beatmap);
        
        var difficultyAttributes = ruleset.CreateDifficultyCalculator(flatWorkingBeatmap).Calculate(scoreInfo.Mods, ct);
        var performanceCalculator = ruleset.CreatePerformanceCalculator();
        if (performanceCalculator != null)
        {
            // Performance calculation might fail on weird maps like Aspire.
            try
            {
                var performanceAttributesTask = performanceCalculator.CalculateAsync(scoreInfo, difficultyAttributes, ct);
                
                if (await Task.WhenAny(performanceAttributesTask, Task.Delay(CalculationTimeout, ct)) ==
                    performanceAttributesTask)
                {
                    
                    await performanceAttributesTask;
                    
                    var performanceAttributes = performanceAttributesTask.Result;
                    logger.Log(LogLevel.Information, "Score ID: {scoreId}, new PP: {pp}", apiScore.Id,
                        (float)performanceAttributes.Total);

                    await scoreCacheRepository.SetScoreCalculatableAsync(apiScore.BeatmapId, apiScore.Mode, true);
                    return (float)performanceAttributes.Total;
                }
                
                await scoreCacheRepository.SetScoreCalculatableAsync(apiScore.BeatmapId, apiScore.Mode, false);
                return null;
            }
            catch (Exception ex)
            {
                logger.Log(LogLevel.Error, ex, "Score calculation failed! Score: {score};", 
                    apiScore.Id);
                await scoreCacheRepository.SetScoreCalculatableAsync(apiScore.BeatmapId, apiScore.Mode, false);
                return null;
            }
        }
        logger.Log(LogLevel.Error, "Score calculation failed! Score ID: {score}; Error: {error}", 
            apiScore.Id, $"{nameof(performanceCalculator)} is null");
        await scoreCacheRepository.SetScoreCalculatableAsync(apiScore.BeatmapId, apiScore.Mode, false);
        return null;
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
}