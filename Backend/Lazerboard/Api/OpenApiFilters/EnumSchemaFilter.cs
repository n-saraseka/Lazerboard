using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lazerboard.Api.OpenApiFilters;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
            return;

        var descriptions = new Dictionary<int, string>();

        foreach (var val in Enum.GetValues(context.Type))
        {
            descriptions[(int)val] = val.ToString()!;
        }

        if (schema.Extensions is null && schema is OpenApiSchema actualSchema)
        {
            actualSchema.Extensions = new Dictionary<string, IOpenApiExtension>();
            actualSchema.Extensions["x-enumDescriptions"] = new EnumDescriptionExtension(descriptions);
        }
    }
}