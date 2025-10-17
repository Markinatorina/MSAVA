using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MSAVA_API.Filters
{
    public class OctetStreamOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var hasOctet = context.MethodInfo
                .GetCustomAttributes(typeof(ConsumesAttribute), false)
                .OfType<ConsumesAttribute>()
                .Any(a => a.ContentTypes.Contains("application/octet-stream"));

            if (!hasOctet)
                return;

            operation.RequestBody = new OpenApiRequestBody
            {
                Required = true,
                Content =
            {
                ["application/octet-stream"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                }
            }
            };
        }
    }
}