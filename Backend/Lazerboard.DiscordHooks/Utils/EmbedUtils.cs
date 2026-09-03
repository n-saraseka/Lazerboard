using Discord;
using Lazerboard.Data.OsuEntities.Enums;

namespace Lazerboard.DiscordHooks.Utils;

public static class EmbedUtils
{
    /// <summary>
    /// Get the <see cref="Mode"/> <see cref="Color"/>
    /// </summary>
    /// <param name="mode">The <see cref="Mode"/></param>
    /// <returns>The respective <see cref="Color"/></returns>
    public static Color GetModeColor(Mode mode)
    {
        switch (mode)
        {
            case Mode.Osu:
                return new Color(200, 120, 120);
            case Mode.Taiko:
                return new Color(200, 180, 60);
            case Mode.Fruits:
                return new Color(120, 200, 100);
            case Mode.Mania:
                return new Color(100, 100, 200);
            default:
                return new Color(200, 200, 200);
        }
    }
    
    /// <summary>
    /// Get the <see cref="Mode"/> text for embed
    /// </summary>
    /// <param name="mode">The <see cref="Mode"/></param>
    /// <returns>The respective string</returns>
    public static string GetModeText(Mode mode)
    {
        switch (mode)
        {
            case Mode.Osu:
                return ":red_circle: osu!";
            case Mode.Taiko:
                return ":drum: osu!taiko";
            case Mode.Fruits:
                return ":green_apple: osu!catch";
            case Mode.Mania:
                return ":musical_keyboard: osu!mania";
            default:
                return "unknown";
        }
    }
    
    /// <summary>
    /// Get the <see cref="BeatmapStatus"/> text for embed
    /// </summary>
    /// <param name="status">The <see cref="BeatmapStatus"/></param>
    /// <returns>The respective string</returns>
    public static string GetStatusText(BeatmapStatus status)
    {
        switch (status)
        {
            case BeatmapStatus.Graveyard:
                return ":headstone: Graveyard";
            case BeatmapStatus.Wip:
                return ":construction: WIP";
            case BeatmapStatus.Pending:
                return ":timer: Pending";
            case BeatmapStatus.Ranked:
                return ":arrow_double_up: Ranked";
            case BeatmapStatus.Approved:
                return ":arrow_double_up: Approved";
            case BeatmapStatus.Qualified:
                return ":white_check_mark: Qualified";
            case BeatmapStatus.Loved:
                return ":heart: Loved";
            default:
                return "unknown";
        }
    }
}