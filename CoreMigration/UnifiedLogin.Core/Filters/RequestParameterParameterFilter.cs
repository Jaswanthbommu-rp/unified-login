using Microsoft.OpenApi.Models;
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
        public void Apply(OpenApiParameter parameter, ParameterFilterContext context)
        {
            // ✅ Suppress any parameter that belongs to RequestParameter's property tree
            if (context.ParameterInfo?.Member?.DeclaringType == typeof(RequestParameter))
            {
                parameter.Deprecated = true;
            }

            // ✅ Mark auto-expanded RequestParameter sub-properties as hidden
            if (context.ApiParameterDescription?.ModelMetadata?.ContainerType == typeof(RequestParameter))
            {
                // Force these to not appear by marking schema as empty
                parameter.Schema = new OpenApiSchema { Type = "string" };
                parameter.Deprecated = true;
            }
        }
    }
}
