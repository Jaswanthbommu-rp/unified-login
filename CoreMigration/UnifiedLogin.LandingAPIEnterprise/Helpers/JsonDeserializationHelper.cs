using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UnifiedLogin.LandingAPIEnterprise.Helpers
{
    /// <summary>
    /// Helper class for safe JSON deserialization
    /// </summary>
    public static class JsonDeserializationHelper
    {
        /// <summary>
        /// Safely deserializes JSON string to object with default fallback
        /// </summary>
        public static T SafeDeserialize<T>(string json, T defaultValue = default) where T : class
        {
            if (string.IsNullOrWhiteSpace(json))
                return defaultValue;

            try
            {
                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (JsonException)
            {
                return defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Safely deserializes JSON string to list
        /// </summary>
        public static IList<T> SafeDeserializeList<T>(string json) where T : class
        {
            var defaultList = new List<T>();
            return SafeDeserialize<IList<T>>(json, defaultList) ?? defaultList;
        }

        /// <summary>
        /// Safely serializes object to JSON string
        /// </summary>
        public static string SafeSerialize<T>(T obj, bool indented = false) where T : class
        {
            if (obj == null)
                return string.Empty;

            try
            {
                var settings = new JsonSerializerSettings
                {
                    Formatting = indented ? Formatting.Indented : Formatting.None
                };
                return JsonConvert.SerializeObject(obj, settings);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Converts JSON string to dictionary for custom field handling
        /// </summary>
        public static Dictionary<string, string> ConvertToDictionary<T>(IList<T> items, 
            Func<T, string> keySelector, Func<T, string> valueSelector) where T : class
        {
            var dictionary = new Dictionary<string, string>();

            if (items == null || items.Count == 0)
                return dictionary;

            foreach (var item in items)
            {
                var key = keySelector(item);
                var value = valueSelector(item);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    dictionary[key] = value ?? string.Empty;
                }
            }

            return dictionary;
        }
    }
}
