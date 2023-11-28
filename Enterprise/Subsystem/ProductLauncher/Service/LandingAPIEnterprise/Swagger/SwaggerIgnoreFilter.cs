using Swashbuckle.Swagger;
using System;
using System.Linq;
using System.Reflection;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Swagger
{
    public class SwaggerIgnoreFilter: ISchemaFilter
    {
        public void Apply(Schema schema, SchemaRegistry schemaRegistry, Type type)
        {
            if (schema?.properties == null || type == null)
                return;

            var excludedProperties = type.GetProperties()
                                         .Where(t =>
                                             t.GetCustomAttribute<SwaggerIgnoreAttribute>()
                                             != null);

            foreach (var excludedProperty in excludedProperties)
            {
                if (schema.properties.ContainsKey(excludedProperty.Name))
                    schema.properties.Remove(excludedProperty.Name);
            }
        }
    }

    public class SwaggerIgnoreAttribute : Attribute
    {
        // This class intentionally left blank
    }
}