using Microsoft.AspNet.Identity.Owin;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using Owin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Handlers;
using System.Net;
using System.Web.Http;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            HttpConfiguration config = new HttpConfiguration();
            // Add our custom no-cache handler to the response
            config.MessageHandlers.Add(new NoCacheHandler());
            WebApiConfig.Register(config);
            app.UseWebApi(config);

            var formatters = config.Formatters;
            formatters.Remove(formatters.XmlFormatter);
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.None;
            json.SerializerSettings.DateFormatHandling = Newtonsoft.Json.DateFormatHandling.MicrosoftDateFormat;
            json.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
            json.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            // Add distributed cache configuration
            ConfigureDistributedCache(app);
        }

        // ... other using directives

        private void ConfigureDistributedCache(IAppBuilder app)
        {
            var services = new ServiceCollection();

            // For Redis
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "rcauneaprds101:6479"; // Replace with your Redis server configuration
                options.InstanceName = "SampleInstance";
            });

            // Or for SQL Server
            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = "rcauneaprds101:6479,password=Psz$h7QFVv#9&38Cn3J#XC3Ryp,syncTimeout=1000,asyncTimeout=1000,allowAdmin=True";
            });

            var serviceProvider = services.BuildServiceProvider();
            app.Use(async (context, next) =>
            {
                context.Set<IDistributedCache>(serviceProvider.GetService<IDistributedCache>());
                await next.Invoke();
            });
        }
    }
}
