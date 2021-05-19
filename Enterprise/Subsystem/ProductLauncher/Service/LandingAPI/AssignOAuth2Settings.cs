using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Description;
using Swashbuckle.Swagger;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPI
{
    /// <summary>
    /// Assigns OAuth2 Settings; required for Swagger
    /// </summary>
    public class AssignOAuth2Settings : IOperationFilter
    {
        /// <summary>
        /// Apply Oauth settings
        /// </summary>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (apiDescription != null)
            {
                var actFilters = apiDescription.ActionDescriptor.GetFilterPipeline();
                var allowsAnonymous = actFilters.Select(f => f.Instance).OfType<OverrideAuthorizationAttribute>().Any();
                if (allowsAnonymous)
                    return; // must be an anonymous method

                if (operation.security == null)
                    operation.security = new List<IDictionary<string, IEnumerable<string>>>();
            }

            if (operation != null)
            {
                if (operation.security == null)
                {
                    operation.security = new List<IDictionary<string, IEnumerable<string>>>();
                }
	            var oAuthRequirements = new Dictionary<string, IEnumerable<string>>()
	            {
		            {"oauth2", ConfigReader.GetRequiredScope.Split(null)},
		            {"Token", new List<string>()}
	            };


				operation.security.Add(oAuthRequirements);
            }
        }
    }

    public class AssignOAuth2SettingsServer : IOperationFilter
    {
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            var scopes = new List<string>() { "rplandingapiserver" }; // For me I just had one scope that is added to all all my methods, you might have to be more selective on how scopes are added.

            if (scopes.Any())
            {
                if (operation.security == null)
                    operation.security = new List<IDictionary<string, IEnumerable<string>>>();

	            var oAuthRequirements = new Dictionary<string, IEnumerable<string>>();
	            oAuthRequirements.Add("oauth2", ConfigReader.GetRequiredScope.Split(null));
	            oAuthRequirements.Add("Token", new List<string>());

				operation.security.Add(oAuthRequirements);
            }
        }
    }
}