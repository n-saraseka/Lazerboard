using System.Security.Cryptography;
using System.Text;
using Lazerboard.Api.Dtos;
using Newtonsoft.Json;

namespace Lazerboard.Api.Utils;

public static class ResponseCachingUtils
{
    /// <summary>
    /// Get score query key for cache
    /// </summary>
    /// <param name="commandHash">The <see cref="ScoreQueryCommand"/> hash code</param>
    /// <param name="amount">Amount of scores to return</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <returns>The cache string</returns>
    public static string GetScoreQueryKeyForCache(string commandHash, int? amount, int? page)
    {
        var pagePortion = page.HasValue ? $"{page.Value}" : "";
        var amountPortion = amount.HasValue ? $"{amount.Value}" : "";

        return $"{commandHash}-{pagePortion}-{amountPortion}";
    }
    
    /// <summary>
    /// Get <see cref="ScoreQueryCommand"/> hash string
    /// </summary>
    /// <param name="command"></param>
    /// <returns>The hash string</returns>
    public static string GetScoreCommandHashForCache(ScoreQueryCommand command)
    {
        command.Modes = command.Modes.OrderBy(m => m).ToArray();
        command.IncludeMods = command.IncludeMods.OrderBy(m => m).ToArray();
        command.ExcludeMods = command.ExcludeMods.OrderBy(m => m).ToArray();

        using var sha256 = SHA256.Create();
        var serializedString = JsonConvert.SerializeObject(command);
        var bytes = Encoding.UTF8.GetBytes(serializedString);
        var hashBytes = sha256.ComputeHash(bytes);
            
        return Convert.ToHexString(hashBytes);
    }

    /// <summary>
    /// Get serialized <see cref="ScoresResponse"/> string
    /// </summary>
    /// <param name="response">The <see cref="ScoresResponse"/></param>
    /// <returns>The serialized string</returns>
    public static string GetSerializedScoresResponse(ScoresResponse response) =>
        JsonConvert.SerializeObject(response,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });

    /// <summary>
    /// Get deserialized <see cref="ScoresResponse"/> from string
    /// </summary>
    /// <param name="response">The serialized string</param>
    /// <param name="isUserScores">Whether the response is a <see cref="UserScoresResponse"/> or not</param>
    /// <returns>The <see cref="ScoresResponse"/> or null</returns>
    public static ScoresResponse? GetDeserializedScoresResponse(string response, bool isUserScores) =>
        isUserScores
            ? JsonConvert.DeserializeObject<UserScoresResponse?>(response,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore })
            : JsonConvert.DeserializeObject<ScoresResponse?>(response,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
    
    /// <summary>
    /// Get serialized <see cref="UserRankingResponse"/> string
    /// </summary>
    /// <param name="response">The <see cref="UserRankingResponse"/></param>
    /// <returns>The string</returns>
    public static string GetSerializedUserRankingResponse(UserRankingResponse response) =>
        JsonConvert.SerializeObject(response);
    
    /// <summary>
    /// Get deserialized <see cref="UserRankingResponse"/> from string
    /// </summary>
    /// <param name="response">The serialized string</param>
    /// <returns>The <see cref="UserRankingResponse"/> or null</returns>
    public static UserRankingResponse? GetDeserializedUserRankingResponse(string response) =>
        JsonConvert.DeserializeObject<UserRankingResponse?>(response,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
    
    /// <summary>
    /// Get user stats key for cache
    /// </summary>
    /// <param name="commandHash">The <see cref="ScoreQueryCommand"/> hash code</param>
    /// <param name="userId">The user ID</param>
    /// <returns>The cache string</returns>
    public static string GetUserStatsKeyForCache(string commandHash, int userId) =>
        $"stats-{userId}-{commandHash}";
    
    /// <summary>
    /// Get serialized <see cref="UserStatsResponse"/> string
    /// </summary>
    /// <param name="response">The <see cref="UserStatsResponse"/></param>
    /// <returns>The serialized string</returns>
    public static string GetSerializedUserStatsResponse(UserStatsResponse response) =>
        JsonConvert.SerializeObject(response);
    
    /// <summary>
    /// Get deserialized <see cref="UserStatsResponse"/> from string
    /// </summary>
    /// <param name="response">The serialized string</param>
    /// <returns>The <see cref="UserStatsResponse"/> or null</returns>
    public static UserStatsResponse? GetDeserializedUserStatsResponse(string response) =>
        JsonConvert.DeserializeObject<UserStatsResponse?>(response,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
}