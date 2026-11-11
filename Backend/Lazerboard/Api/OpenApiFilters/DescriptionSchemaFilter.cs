using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lazerboard.Api.OpenApiFilters;

public class DescriptionSchemaFilter : ISchemaFilter
{
    private static readonly Regex NameRegex = new Regex(
        @"(?:[A-Za-z_][A-Za-z0-9_]*\.)+([A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled);

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Description is null) return;

        schema.Description = NameRegex.Replace(schema.Description, "$1");
    }
}