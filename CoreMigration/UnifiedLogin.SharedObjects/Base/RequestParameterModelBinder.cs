using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using UnifiedLogin.SharedObjects.Base;

namespace UnifiedLogin.SharedObjects.Base
{
    /// <summary>
    /// Custom model binder for RequestParameter.
    /// Handles the format: ?datafilter.filterBy={"key":"value"}&datafilter.pages.startRow=1
    /// Works dynamically regardless of the parameter name used in the action method.
    /// </summary>
    public class RequestParameterModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            // ✅ Get the parameter name with multiple fallback strategies
            var modelName = GetModelName(bindingContext);

            var valueProvider = bindingContext.ValueProvider;

            var requestParameter = new RequestParameter
            {
                FilterBy = new Dictionary<string, string>(),
                SortBy = new Dictionary<string, string>(),
                Pages = new PageRequest { StartRow = 0, ResultsPerPage = 100 }
            };

            // ✅ Try binding with the detected model name
            if (!string.IsNullOrEmpty(modelName))
            {
                TryBindWithPrefix(modelName, valueProvider, requestParameter);
            }

            // ✅ Fallback: scan all query string keys to find the actual prefix
            if (requestParameter.FilterBy.Count == 0 && requestParameter.SortBy.Count == 0)
            {
                var query = bindingContext.HttpContext.Request.Query;
                var detectedPrefix = DetectPrefixFromQuery(query);

                if (!string.IsNullOrEmpty(detectedPrefix))
                {
                    TryBindFromQuery(query, detectedPrefix, requestParameter);
                }
                else
                {
                    TryBindFromQuery(query, requestParameter);
                }
            }

            // ✅ Ensure no nulls
            requestParameter.FilterBy ??= new Dictionary<string, string>();
            requestParameter.SortBy ??= new Dictionary<string, string>();
            requestParameter.Pages ??= new PageRequest { StartRow = 0, ResultsPerPage = 100 };

            bindingContext.Result = ModelBindingResult.Success(requestParameter);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets the model name using multiple fallback strategies
        /// </summary>
        private static string GetModelName(ModelBindingContext bindingContext)
        {
            // Strategy 1: Use ModelName if available
            if (!string.IsNullOrEmpty(bindingContext.ModelName))
            {
                return bindingContext.ModelName;
            }

            // Strategy 2: Get from ParameterDescriptor
            if (bindingContext.ActionContext?.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor actionDescriptor)
            {
                var parameter = actionDescriptor.Parameters
                    .FirstOrDefault(p => p.ParameterType == typeof(RequestParameter));
                
                if (parameter != null && !string.IsNullOrEmpty(parameter.Name))
                {
                    return parameter.Name;
                }
            }

            // Strategy 3: Get from FieldName
            if (!string.IsNullOrEmpty(bindingContext.FieldName))
            {
                return bindingContext.FieldName;
            }

            // Strategy 4: Default fallback
            return "datafilter";
        }

        /// <summary>
        /// Detects the prefix from query string by finding keys with .filterBy or .sortBy
        /// </summary>
        private static string? DetectPrefixFromQuery(Microsoft.AspNetCore.Http.IQueryCollection query)
        {
            foreach (var key in query.Keys)
            {
                var keyLower = key.ToLower();
                
                // Look for patterns like "datafilter.filterby" or "filter.sortby"
                if (keyLower.Contains(".filterby") || keyLower.Contains(".sortby") || 
                    keyLower.Contains(".pages.startrow") || keyLower.Contains(".pages.resultsperpage"))
                {
                    var dotIndex = keyLower.IndexOf('.');
                    if (dotIndex > 0)
                    {
                        return key.Substring(0, dotIndex);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Tries to bind using the value provider with a specific prefix
        /// </summary>
        private static void TryBindWithPrefix(string modelName, IValueProvider valueProvider, RequestParameter requestParameter)
        {
            // Try camelCase first
            var filterByValue = valueProvider.GetValue($"{modelName}.filterBy");
            if (filterByValue != ValueProviderResult.None && !string.IsNullOrEmpty(filterByValue.FirstValue))
            {
                requestParameter.FilterBy = ParseJsonToDictionary(filterByValue.FirstValue);
            }

            var sortByValue = valueProvider.GetValue($"{modelName}.sortBy");
            if (sortByValue != ValueProviderResult.None && !string.IsNullOrEmpty(sortByValue.FirstValue))
            {
                requestParameter.SortBy = ParseJsonToDictionary(sortByValue.FirstValue);
            }

            var startRowValue = valueProvider.GetValue($"{modelName}.pages.startRow");
            if (startRowValue != ValueProviderResult.None && int.TryParse(startRowValue.FirstValue, out int startRow))
            {
                requestParameter.Pages.StartRow = startRow;
            }

            var resultsPerPageValue = valueProvider.GetValue($"{modelName}.pages.resultsPerPage");
            if (resultsPerPageValue != ValueProviderResult.None && int.TryParse(resultsPerPageValue.FirstValue, out int resultsPerPage))
            {
                requestParameter.Pages.ResultsPerPage = resultsPerPage;
            }

            // Try PascalCase if camelCase didn't work
            if (requestParameter.FilterBy.Count == 0)
            {
                var filterByPascal = valueProvider.GetValue($"{modelName}.FilterBy");
                if (filterByPascal != ValueProviderResult.None && !string.IsNullOrEmpty(filterByPascal.FirstValue))
                {
                    requestParameter.FilterBy = ParseJsonToDictionary(filterByPascal.FirstValue);
                }
            }

            if (requestParameter.SortBy.Count == 0)
            {
                var sortByPascal = valueProvider.GetValue($"{modelName}.SortBy");
                if (sortByPascal != ValueProviderResult.None && !string.IsNullOrEmpty(sortByPascal.FirstValue))
                {
                    requestParameter.SortBy = ParseJsonToDictionary(sortByPascal.FirstValue);
                }
            }

            if (requestParameter.Pages.StartRow == 0)
            {
                var startRowPascal = valueProvider.GetValue($"{modelName}.Pages.StartRow");
                if (startRowPascal != ValueProviderResult.None && int.TryParse(startRowPascal.FirstValue, out int startRowP))
                {
                    requestParameter.Pages.StartRow = startRowP;
                }
            }

            if (requestParameter.Pages.ResultsPerPage == 100)
            {
                var resultsPerPagePascal = valueProvider.GetValue($"{modelName}.Pages.ResultsPerPage");
                if (resultsPerPagePascal != ValueProviderResult.None && int.TryParse(resultsPerPagePascal.FirstValue, out int resultsPerPageP))
                {
                    requestParameter.Pages.ResultsPerPage = resultsPerPageP;
                }
            }
        }

        /// <summary>
        /// Tries to bind directly from query collection when prefix is known
        /// </summary>
        private static void TryBindFromQuery(Microsoft.AspNetCore.Http.IQueryCollection query, string prefix, RequestParameter requestParameter)
        {
            var prefixLower = prefix.ToLower();

            foreach (var key in query.Keys)
            {
                var keyLower = key.ToLower();

                if (keyLower == $"{prefixLower}.filterby")
                {
                    requestParameter.FilterBy = ParseJsonToDictionary(query[key].ToString());
                }
                else if (keyLower == $"{prefixLower}.sortby")
                {
                    requestParameter.SortBy = ParseJsonToDictionary(query[key].ToString());
                }
                else if (keyLower == $"{prefixLower}.pages.startrow")
                {
                    if (int.TryParse(query[key].ToString(), out int sr))
                        requestParameter.Pages.StartRow = sr;
                }
                else if (keyLower == $"{prefixLower}.pages.resultsperpage")
                {
                    if (int.TryParse(query[key].ToString(), out int rpp))
                        requestParameter.Pages.ResultsPerPage = rpp;
                }
            }
        }
        /// <summary>
        /// Tries to bind directly from query collection WITHOUT a prefix (e.g., ?FilterBy=...&Pages.StartRow=1)
        /// This handles the case where the client sends parameters directly without "datafilter." prefix
        /// </summary>
        private static void TryBindFromQuery(Microsoft.AspNetCore.Http.IQueryCollection query, RequestParameter requestParameter)
        {
            foreach (var key in query.Keys)
            {
                // ✅ Use case-insensitive comparison for both camelCase and PascalCase
                if (key.Equals("FilterBy", StringComparison.OrdinalIgnoreCase))
                {
                    requestParameter.FilterBy = ParseJsonToDictionary(query[key].ToString());
                }
                else if (key.Equals("SortBy", StringComparison.OrdinalIgnoreCase))
                {
                    requestParameter.SortBy = ParseJsonToDictionary(query[key].ToString());
                }
                else if (key.Equals("Pages.StartRow", StringComparison.OrdinalIgnoreCase) || 
                         key.Equals("StartRow", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(query[key].ToString(), out int sr))
                        requestParameter.Pages.StartRow = sr;
                }
                else if (key.Equals("Pages.ResultsPerPage", StringComparison.OrdinalIgnoreCase) || 
                         key.Equals("ResultsPerPage", StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(query[key].ToString(), out int rpp))
                        requestParameter.Pages.ResultsPerPage = rpp;
                }
            }
        }
        /// <summary>
        /// Parses a JSON string into Dictionary&lt;string, string&gt;.
        /// Example: {"status":"1","offsetMinutes":"0"} -> {["status","1"],["offsetMinutes","0"]}
        /// </summary>
        private static Dictionary<string, string> ParseJsonToDictionary(string jsonString)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(jsonString))
                return result;

            jsonString = jsonString.Trim();

            // Strip surrounding quotes if present
            if (jsonString.StartsWith('"') && jsonString.EndsWith('"'))
                jsonString = jsonString[1..^1];

            try
            {
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    foreach (var property in root.EnumerateObject())
                    {
                        result[property.Name] = property.Value.ValueKind switch
                        {
                            JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                            JsonValueKind.Number => property.Value.GetRawText(),
                            JsonValueKind.True => "true",
                            JsonValueKind.False => "false",
                            JsonValueKind.Null => string.Empty,
                            _ => property.Value.GetRawText()
                        };
                    }
                }
            }
            catch (JsonException)
            {
                // Fallback: treat entire string as a single value
                result["value"] = jsonString;
            }

            return result;
        }
    }
}