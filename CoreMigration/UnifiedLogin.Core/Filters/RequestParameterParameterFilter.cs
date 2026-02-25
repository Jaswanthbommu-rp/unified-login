using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Text;
using UnifiedLogin.SharedObjects.Base;

namespace UnifiedLogin.Core.Filters
{
    /// <summary>
    /// Parameter filter that suppresses individual RequestParameter property expansion.
    /// This prevents Swagger from showing FilterBy, SortBy, Pages.StartRow etc. as separate parameters.
    /// </summary>
    public class RequestParameterParameterFilter : IParameterFilter
    {
        public void Apply(IOpenApiParameter parameter, ParameterFilterContext context)
        {
            // ✅ Suppress any parameter that belongs to RequestParameter's property tree
            if (context.ParameterInfo?.Member?.DeclaringType == typeof(RequestParameter))
            {
                if (parameter is OpenApiParameter op1)
                    op1.Deprecated = true;
            }

            // ✅ Mark auto-expanded RequestParameter sub-properties as hidden
            if (context.ApiParameterDescription?.ModelMetadata?.ContainerType == typeof(RequestParameter))
            {
                // Force these to not appear by marking schema as empty
                if (parameter is OpenApiParameter p)
                {
                    p.Schema = new OpenApiSchema { Type = JsonSchemaType.String };
                    p.Deprecated = true;
                }
            }
        }
    }
}
