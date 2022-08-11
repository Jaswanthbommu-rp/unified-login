using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Newtonsoft.Json.Serialization;
using Owin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Handlers;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using System.Net;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Web.Http;
using System.Web.Http.Cors;
using RealPage.IdentityServer4.AccessTokenValidation;

[assembly: OwinStartup("RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Startup", typeof(RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise.Startup))]

namespace RP.Enterprise.Subsystem.ProductLauncher.Service.LandingAPIEnterprise
{
    public class Startup
    {
        /// <summary>
        /// Used to support OAuth authentication 
        /// </summary>
        /// <param name="app"></param>
        public void Configuration(IAppBuilder app)
        {

            System.Net.ServicePointManager.SecurityProtocol =
                SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            app.UseIdentityServerBearerTokenAuthentication(new IdentityServerBearerTokenAuthenticationOptions
            {
                Authority = ConfigReader.GetIssuerUri,
                RequiredScopes = ConfigReader.GetRequiredScope.Split(null),
                DelayLoadMetadata = true,
                EnableValidationResultCache = true,
                //SigningCertificate = GetSigningCertificate(),
                IssuerName = ConfigReader.GetIssuerUri,
                //ClientSecret = ConfigReader.GetApiSecret
                //ValidationMode = ValidationMode.ValidationEndpoint,
            });
            app.UseStageMarker(PipelineStage.Authenticate);

            var config = new HttpConfiguration();
            //app.UseCookieAuthentication(new CookieAuthenticationOptions
            //{
            //    AuthenticationType = "Cookies"
            //});

            // Add our custom no-cache handler to the response
            config.MessageHandlers.Add(new NoCacheHandler());

            WebApiConfig.Register(config);

            // cors calls have been moved to the BaseApiController class
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);
            //app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            app.UseWebApi(config);

            if (!(ConfigReader.Environment == "PROD"))
            {
                SwaggerConfig.Register(config);
            }

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
