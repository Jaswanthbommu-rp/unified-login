using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;
using UnifiedLogin.SharedObjects.Base;

namespace UnifiedLogin.Core.Filters
{
    /// <summary>
    /// Schema filter to customize how RequestParameter is displayed in Swagger schemas
    /// </summary>
    public class RequestParameterSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(RequestParameter) && schema is OpenApiSchema s)
            {
                // ✅ Clear all properties to prevent them from showing in Swagger
                s.Properties?.Clear();
                s.Type = JsonSchemaType.Object;
                s.Description = "Complex filter object - use datafilter.* query parameters instead";
            }
            //if (context.Type != typeof(RequestParameter))
            //    return;

            //// Clear default properties
            //schema.Properties.Clear();

            //// Add custom properties with better descriptions
            //schema.Properties.Add("filterBy", new OpenApiSchema
            //{
            //    Type = "object",
            //    Description = "Filter criteria as key-value pairs. Example: {\"status\":\"1\",\"name\":\"John\"}",
            //    AdditionalPropertiesAllowed = true,
            //    AdditionalProperties = new OpenApiSchema { Type = "string" }
            //});

            //schema.Properties.Add("sortBy", new OpenApiSchema
            //{
            //    Type = "object",
            //    Description = "Sort criteria as key-value pairs. Example: {\"firstName\":\"ASC\",\"lastName\":\"DESC\"}",
            //    AdditionalPropertiesAllowed = true,
            //    AdditionalProperties = new OpenApiSchema { Type = "string" }
            //});

            //schema.Properties.Add("pages", new OpenApiSchema
            //{
            //    Type = "object",
            //    Properties = new Dictionary<string, OpenApiSchema>
            //    {
            //        ["startRow"] = new OpenApiSchema
            //        {
            //            Type = "integer",
            //            Format = "int32",
            //            Default = new Microsoft.OpenApi.Any.OpenApiInteger(0),
            //            Description = "Starting row (0-based)"
            //        },
            //        ["resultsPerPage"] = new OpenApiSchema
            //        {
            //            Type = "integer",
            //            Format = "int32",
            //            Default = new Microsoft.OpenApi.Any.OpenApiInteger(100),
            //            Description = "Number of results per page"
            //        }
            //    }
            //});
        }
    }
}
