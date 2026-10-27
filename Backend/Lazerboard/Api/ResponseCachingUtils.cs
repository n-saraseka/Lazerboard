using System.Security.Cryptography;
using System.Text;
using Lazerboard.Api.Dtos;
using Newtonsoft.Json;

namespace Lazerboard.Api;

public static class ResponseCachingUtils
{
    /// <summary>
    /// Get score query key for cache
    /// </summary>
    /// <param name="commandHash">The <see cref="ScoreQueryCommand"/> hash code</param>
    /// <param name="amount">Amount of scores to return</param>
    /// <param name="page">Page (defaults to 1)</param>
    /// <returns></returns>
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
    /// <returns>The string</returns>
    public static string GetSerializedScoresResponse(ScoresResponse response) =>
        JsonConvert.SerializeObject(response,
            new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
    
    /// <summary>
    /// Get deserialized <see cref="ScoresResponse"/> from string
    /// </summary>
    /// <param name="response">The serialized string</param>
    /// <returns>The <see cref="ScoresResponse"/> or null</returns>
    public static ScoresResponse? GetDeserializedScoresResponse(string response) =>
        JsonConvert.DeserializeObject<ScoresResponse?>(response,
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
    /// <returns>The <see cref="ScoresResponse"/> or null</returns>
    public static UserRankingResponse? GetDeserializedUserRankingResponse(string response) =>
        JsonConvert.DeserializeObject<UserRankingResponse?>(response,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
}