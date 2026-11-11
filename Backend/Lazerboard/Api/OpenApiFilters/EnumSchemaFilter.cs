using System.ComponentModel;
using System.Reflection;
using System.Text;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lazerboard.Api.OpenApiFilters;

public class EnumSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (!context.Type.IsEnum)
            return;

        var str = "<p>Values:</p><table>";

        foreach (var val in Enum.GetValues(context.Type))
        {
            var description = val.ToString();
            
            str += $"<tr><td>{(int)val}</td><td>{description}</td></tr>";
        }

        str += "</table>";
        
        schema.Description = str;
    }
}