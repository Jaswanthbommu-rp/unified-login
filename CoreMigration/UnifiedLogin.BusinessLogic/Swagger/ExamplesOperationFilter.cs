using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnifiedLogin.SharedObjects;
using UnifiedLogin.SharedObjects.Attribute;
using Swashbuckle.Swagger;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http.Description;


namespace UnifiedLogin.BusinessLogic.Swagger
{
	/// <summary>
	/// Used to generate examples of various result objects that will be documented
	/// </summary>
	public class ExamplesOperationFilter : IOperationFilter
    {
        /// <summary>
        /// Used to decorate webapi methods with example data that can be used in the swagger UI webpage
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="schemaRegistry"></param>
        /// <param name="apiDescription"></param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters != null)
            {
                var parameters = operation.parameters.Select(p => CultureInfo.InvariantCulture.TextInfo.ToTitleCase(p.name));
                operation.operationId = string.Format("{0}By{1}", operation.operationId, string.Join("And", parameters));
				if (operation.parameters.Any(p => p.name == "datafilter.filterBy"))
				{
					foreach ( Parameter p in operation.parameters)
					{
						if (p.name.StartsWith("datafilter."))
						{
							p.type = "string";
						}
					}
				}
            }

            var responseAttributes = apiDescription.GetControllerAndActionAttributes<SwaggerResponseExamplesAttribute>();

            foreach (var attr in responseAttributes)
            {
                var schema = schemaRegistry.GetOrRegister(attr.ResponseType);

                // if response throws an exception, make sure that one of the SwaggerResponses uses the same type as the SwaggerResponseExamples being used 
                var response = operation.responses.FirstOrDefault(x => x.Value.schema.type == schema.type && x.Value.schema.@ref == schema.@ref).Value;

                if (response != null)
                {
                    var provider = (IProvideExamples)Activator.CreateInstance(attr.ExamplesType);
                    response.examples = FormatAsJson(provider);
                }
            }
        }

        #region Private methods
        /// <summary>
        /// Used to convert the example object into json used by Swagger
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        private static object FormatAsJson(IProvideExamples provider)
        {
            var examples = new Dictionary<string, object>()
            {
                {
                    "application/json", provider.GetExamples()
                }
            };
            return ConvertToCamelCase(examples);
        }
        /// <summary>
        /// Used to deserialize objects into json to be used in the Swagger UI to show examples of data
        /// </summary>
        /// <param name="examples"></param>
        /// <returns></returns>
        private static object ConvertToCamelCase(Dictionary<string, object> examples)
        {
            var jsonString = JsonConvert.SerializeObject(examples, new JsonSerializerSettings() { ContractResolver = new CamelCasePropertyNamesContractResolver() });
            return JsonConvert.DeserializeObject(jsonString);
        }
        #endregion
    }
}
