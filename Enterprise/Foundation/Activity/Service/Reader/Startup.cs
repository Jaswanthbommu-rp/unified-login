using System;
using System.Net;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Owin;
using Owin;
using RP.Enterprise.Foundation.Activity.Service.Logging.Reader;
using IdentityServer3.AccessTokenValidation;
using Newtonsoft.Json.Serialization;
using RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Helper;
using RealPage.Logging.Serilog;

[assembly: OwinStartup(typeof(Startup))]

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            SerilogHelpers.ConfigureSerilog("Unified Login");

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

			HttpConfiguration config = new HttpConfiguration();

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = ConfigReader.GetIssuerUri,
                DelayLoadMetadata = true,
                RequiredScopes = new[] { ConfigReader.GetRequiredScope },
            });

            config.MessageHandlers.Add(new NoCacheHandler());
            WebApiConfig.Register(config);

            if (!ConfigReader.Environment.Equals("PROD", StringComparison.OrdinalIgnoreCase))
            {
                    SwaggerConfig.Register(config);
            }


            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors); 
 
            app.UseWebApi(config);

            var formatters = config.Formatters;
            formatters.Remove(formatters.XmlFormatter);

            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
            json.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
            json.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
