using System.Text.RegularExpressions;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lazerboard.Api.OpenApiFilters;

public class SummaryDescriptionOperationFilter : IOperationFilter
{
    private static readonly Regex NameRegex = new Regex(
        @"(?:[A-Za-z_][A-Za-z0-9_]*\.)+([A-Za-z_][A-Za-z0-9_]*)",
        RegexOptions.Compiled);
    
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Summary is not null)
        {
            operation.Summary = NameRegex.Replace(operation.Summary, "$1");
        }

        if (operation.Description is not null)
        {
            operation.Description = NameRegex.Replace(operation.Description, "$1");
        }

        if (operation.RequestBody is not null)
        {
            if (operation.RequestBody.Description is not null)
            {
                operation.RequestBody.Description = NameRegex.Replace(operation.RequestBody.Description, "$1");
            }
        }
    }
}