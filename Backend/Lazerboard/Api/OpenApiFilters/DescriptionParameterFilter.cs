using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lazerboard.Api.OpenApiFilters;

public class DescriptionParameterFilter : IParameterFilter
{
    private static readonly Regex NameRegex = new Regex(
        @"(?:[A-Za-z_][A-Za-z0-9_]*\.)+([A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled);
    
    public void Apply(IOpenApiParameter parameter, ParameterFilterContext context)
    {
        if (parameter.Description is null) return;

        parameter.Description = NameRegex.Replace(parameter.Description, "$1");
    }
}