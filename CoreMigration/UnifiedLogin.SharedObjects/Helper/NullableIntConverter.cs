using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnifiedLogin.SharedObjects.Helper
{
    /// <summary>
    /// Converts JSON string/null/empty values to int, defaulting to 0 when the value
    /// cannot be parsed (e.g. empty string "" sent by the UI for partyRole.roleTypeId).
    /// </summary>
    public class NullableIntConverter : JsonConverter<int>
    {
        public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.Undefined)
                return 0;

            if (reader.TokenType == JsonToken.String)
            {
                var raw = reader.Value?.ToString();
                return int.TryParse(raw, out int parsed) ? parsed : 0;
            }

            if (reader.TokenType == JsonToken.Integer)
                return Convert.ToInt32(reader.Value);

            return 0;
        }

        public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
            => writer.WriteValue(value);
    }
}
