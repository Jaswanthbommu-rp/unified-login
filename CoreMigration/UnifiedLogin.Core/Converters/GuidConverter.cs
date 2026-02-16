using Newtonsoft.Json;
using System;

namespace UnifiedLogin.Core.Converters
{
    /// <summary>
    /// Custom JSON converter for Guid that handles empty strings and null values
    /// Converts empty strings to Guid.Empty for backward compatibility with legacy APIs
    /// </summary>
    public class GuidConverter : JsonConverter<Guid>
    {
        public override Guid ReadJson(JsonReader reader, Type objectType, Guid existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return Guid.Empty;
            }

            if (reader.TokenType == JsonToken.String)
            {
                var value = reader.Value?.ToString();

                // Handle empty strings as Guid.Empty
                if (string.IsNullOrWhiteSpace(value))
                {
                    return Guid.Empty;
                }

                // Try to parse the Guid string
                if (Guid.TryParse(value, out Guid result))
                {
                    return result;
                }

                // If parsing fails, throw an exception with a helpful message
                throw new JsonSerializationException($"Unable to convert \"{value}\" to Guid. Expected a valid GUID format or empty string.");
            }

            throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing Guid.");
        }

        public override void WriteJson(JsonWriter writer, Guid value, JsonSerializer serializer)
        {
            // Write Guid as string in standard format
            writer.WriteValue(value.ToString());
        }
    }

    /// <summary>
    /// Custom JSON converter for nullable Guid that handles empty strings and null values
    /// </summary>
    public class NullableGuidConverter : JsonConverter<Guid?>
    {
        public override Guid? ReadJson(JsonReader reader, Type objectType, Guid? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            if (reader.TokenType == JsonToken.String)
            {
                var value = reader.Value?.ToString();

                // Handle empty strings as null
                if (string.IsNullOrWhiteSpace(value))
                {
                    return null;
                }

                // Try to parse the Guid string
                if (Guid.TryParse(value, out Guid result))
                {
                    return result;
                }

                // If parsing fails, throw an exception with a helpful message
                throw new JsonSerializationException($"Unable to convert \"{value}\" to Guid. Expected a valid GUID format, empty string, or null.");
            }

            throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing nullable Guid.");
        }

        public override void WriteJson(JsonWriter writer, Guid? value, JsonSerializer serializer)
        {
            if (value.HasValue)
            {
                writer.WriteValue(value.Value.ToString());
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
