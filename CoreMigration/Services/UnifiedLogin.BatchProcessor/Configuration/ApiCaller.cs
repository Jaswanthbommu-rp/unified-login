using System.Net.Http.Json;
using System.Text.Json;

namespace UnifiedLogin.BatchProcessor.Configuration;

/// <summary>
/// Static API caller utility for making HTTP POST requests.
/// Converted from .NET Framework 4.8 to .NET Core 10.
/// Replaces Newtonsoft.Json with System.Text.Json.
/// </summary>
public static class ApiCaller
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Posts data to an API endpoint and returns the deserialized response.
    /// Original method signature preserved: PostApi&lt;T, TK&gt;(TK inputObject, string apiPathAndQuery)
    /// </summary>
    /// <typeparam name="T">The expected response type.</typeparam>
    /// <typeparam name="TK">The request payload type.</typeparam>
    /// <param name="inputObject">The object to send in the request body.</param>
    /// <param name="apiPathAndQuery">The API endpoint URL.</param>
    /// <returns>Deserialized response object, or null if the request fails.</returns>
    public static async Task<T> PostApi<T, TK>(TK inputObject, string apiPathAndQuery) where T : class
    {
        T results = null;

        using (var client = new HttpClient())
        {
            var response = await client.PostAsJsonAsync(apiPathAndQuery, inputObject);
            var jsonContent = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                if (IsValidJson(jsonContent)) // Check if content is a valid JSON object/array
                {
                    try
                    {
                        results = JsonSerializer.Deserialize<T>(jsonContent, _jsonOptions);
                    }
                    catch (JsonException)
                    {
                        return jsonContent as T;
                    }
                }
                else
                {
                    return jsonContent as T;
                }
            }
            
            return results;
        }
    }
    /// <summary>
    /// Checks if a string is valid JSON (object or array).
    /// </summary>
    private static bool IsValidJson(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return false;
        }

        content = content.Trim();

        // Quick check: JSON objects start with { and arrays with [
        // Plain strings would be wrapped in quotes, but typically APIs return unwrapped strings
        if ((content.StartsWith("{") && content.EndsWith("}")) || (content.StartsWith("[") && content.EndsWith("]")))
        {
            try
            {
                using (JsonDocument.Parse(content))
                {
                    return true;
                }
            }
            catch (JsonException)
            {
                return false;
            }
        }
        return false;
    }
}
