using Microsoft.OpenApi;

namespace Lazerboard.Api.OpenApiFilters;

public class EnumDescriptionExtension(Dictionary<int, string> descriptions) : IOpenApiExtension
{
    public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
    {
        writer.WriteStartObject();
        foreach (var kvp in descriptions)
        {
            writer.WritePropertyName(kvp.Key.ToString());
            writer.WriteValue(kvp.Value);
        }
        writer.WriteEndObject();
    }
}