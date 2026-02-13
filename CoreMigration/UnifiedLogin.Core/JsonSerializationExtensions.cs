using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnifiedLogin.Core.Converters;

namespace UnifiedLogin.Core;

/// <summary>
/// Extension methods for configuring JSON serialization
/// </summary>
public static class JsonSerializationExtensions
{
    /// <summary>
    /// Configures Newtonsoft.Json for ASP.NET Core controllers with backward compatibility support
    /// </summary>
    /// <param name="builder">The IMvcBuilder instance</param>
    /// <returns>The IMvcBuilder for method chaining</returns>
    public static IMvcBuilder AddNewtonsoftJsonConfiguration(this IMvcBuilder builder)
    {
        return builder.AddNewtonsoftJson(options =>
        {
            // Use DefaultContractResolver to respect [JsonProperty] attributes
            // This configuration supports BOTH camelCase and PascalCase:
            // - Incoming JSON can use either "firstName" or "FirstName"
            // - Outgoing JSON will use the [JsonProperty] attribute names (PascalCase for legacy compatibility)
            // - Newtonsoft.Json performs case-insensitive matching during deserialization by default
            //options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            // Allow enum string values in JSON (instead of numeric values)
            options.SerializerSettings.Converters.Add(new StringEnumConverter());

            // Handle Guid conversion with empty string support (empty string -> Guid.Empty)
            options.SerializerSettings.Converters.Add(new GuidConverter());
            options.SerializerSettings.Converters.Add(new NullableGuidConverter());

            // Ignore null values as specified in model [JsonProperty] attributes
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            // Ignore missing members to be lenient with incoming JSON payloads
            options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            // Preserve reference handling to avoid circular reference errors
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            // Use default formatting for better compatibility
            options.SerializerSettings.Formatting = Formatting.None;
        });
    }
}