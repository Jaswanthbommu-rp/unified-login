using System;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using System.Web.Http.Cors;
using Microsoft.Owin;
using Owin;
using RP.Enterprise.Foundation.Activity.Service.Logging.Reader;
using Newtonsoft.Json.Serialization;
using RP.Enterprise.Foundation.Activity.Service.Logging.Reader.Helper;
using RealPage.Logging.Serilog;
using Elastic.Apm.AspNetFullFramework;
using Elastic.Apm;
using RealPage.IdentityServer4.AccessTokenValidation;

[assembly: OwinStartup(typeof(Startup))]

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            SerilogHelpers.ConfigureSerilog("Unified Login");

            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

			HttpConfiguration config = new HttpConfiguration();

            // set up agent with components
            var agentComponents = ElasticApmModule.CreateAgentComponents();
            Agent.Setup(agentComponents);

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = ConfigReader.GetIssuerUri,
                DelayLoadMetadata = true,
                RequiredScopes = new[] { ConfigReader.GetRequiredScope },
                EnableValidationResultCache = true,
                PreserveAccessToken = true,
                IssuerName = ConfigReader.GetIssuerUri,

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

        private static X509Certificate2 GetSigningCertificate()
        {
            //#if DEBUG
            //            return new X509Certificate2(AppDomain.CurrentDomain.BaseDirectory + "\\bin\\SomeCert.pfx", "idsrv3test");
            //#endif
            var certStore = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            certStore.Open(OpenFlags.ReadOnly | OpenFlags.IncludeArchived);
            // IF THE CERT CAN'T BE FOUND MAKE SURE IT DOESN'T HAVE THE HIDDEN UNICODE CHARACTER IN THE BEGINNING IN THE WEB.CONFIG!
            var certCollection = certStore.Certificates.Find(
                X509FindType.FindByThumbprint, ConfigReader.GetIdentityServerSigningCertThumbprint, false);
            // Get the first cert with the thumbprint
            if (certCollection.Count > 0)
            {
                var cert = certCollection[0];
                certStore.Close();
                // Use certificate
                return cert;
            }
            certStore.Close();
            throw new SecurityException("No certificate specified or found for IdentityServer-" + certCollection.Count.ToString() + "-" + ConfigReader.GetIdentityServerSigningCertThumbprint);
        }
    }
}
