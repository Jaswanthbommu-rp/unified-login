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
		}
	}
}
