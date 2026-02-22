using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnifiedLogin.SharedObjects.Extensions
{
    /// <summary>
    /// Converts DateTime to Microsoft JSON date format /Date(ticks)/
    /// for backward compatibility with .NET Framework 4.8
    /// </summary>
    public class MicrosoftDateFormatConverter : DateTimeConverterBase
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            DateTime dateTime;
            if (value is DateTime dt)
            {
                dateTime = dt;
            }
            else if (value is DateTimeOffset dto)
            {
                dateTime = dto.UtcDateTime;
            }
            else
            {
                throw new JsonSerializationException($"Expected DateTime or DateTimeOffset object value, got {value.GetType()}");
            }

            // Convert to UTC if not already
            if (dateTime.Kind == DateTimeKind.Local)
            {
                dateTime = dateTime.ToUniversalTime();
            }
            else if (dateTime.Kind == DateTimeKind.Unspecified)
            {
                dateTime = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
            }

            // Calculate milliseconds since Unix epoch
            long milliseconds = (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalMilliseconds;

            // Write in Microsoft JSON date format
            writer.WriteValue($"/Date({milliseconds})/");
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (objectType == typeof(DateTime?))
                {
                    return null;
                }
                return DateTime.MinValue;
            }

            if (reader.TokenType == JsonToken.String)
            {
                string dateText = reader.Value?.ToString();

                if (string.IsNullOrEmpty(dateText))
                {
                    return objectType == typeof(DateTime?) ? (DateTime?)null : DateTime.MinValue;
                }

                // Parse /Date(ticks)/ format
                if (dateText.StartsWith("/Date(") && dateText.EndsWith(")/"))
                {
                    string ticksString = dateText.Substring(6, dateText.Length - 8);

                    // Handle timezone offset like /Date(1234567890+0500)/
                    int plusIndex = ticksString.IndexOf('+');
                    int minusIndex = ticksString.IndexOf('-');
                    int offsetIndex = plusIndex > 0 ? plusIndex : minusIndex;

                    if (offsetIndex > 0)
                    {
                        ticksString = ticksString.Substring(0, offsetIndex);
                    }

                    if (long.TryParse(ticksString, out long milliseconds))
                    {
                        return UnixEpoch.AddMilliseconds(milliseconds);
                    }
                }

                // Try standard DateTime parsing as fallback
                if (DateTime.TryParse(dateText, out DateTime result))
                {
                    return result;
                }
            }
            else if (reader.TokenType == JsonToken.Date)
            {
                return reader.Value;
            }

            throw new JsonSerializationException($"Unexpected token parsing date. Expected String or Date, got {reader.TokenType}.");
        }
    }
}
