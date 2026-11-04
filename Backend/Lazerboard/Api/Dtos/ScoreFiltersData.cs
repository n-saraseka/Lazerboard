using Npgsql;

namespace Lazerboard.Api.Dtos;

public class ScoreFiltersData
{
    public string FilterText { get; set; } = string.Empty;
    public object[] Parameters = [];
}