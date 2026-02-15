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
           // options.SerializerSettings.Converters.Add(new StringEnumConverter());
            options.SerializerSettings.Converters.Add(new UnifiedLogin.SharedObjects.Extensions.MicrosoftDateFormatConverter());
            // Handle Guid conversion with empty string support (empty string -> Guid.Empty)
            options.SerializerSettings.Converters.Add(new GuidConverter());
            options.SerializerSettings.Converters.Add(new NullableGuidConverter());

            // BACKWARD COMPATIBILITY FIX: Include null values like .NET Framework 4.8
            // This ensures properties with null values appear in the JSON output
            // Individual properties can override this using [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
            options.SerializerSettings.NullValueHandling = NullValueHandling.Include;

            // Ignore missing members to be lenient with incoming JSON payloads
            options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            // Preserve reference handling to avoid circular reference errors
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include;

            // Use default formatting for better compatibility
            options.SerializerSettings.Formatting = Formatting.None;
        });
    }
}