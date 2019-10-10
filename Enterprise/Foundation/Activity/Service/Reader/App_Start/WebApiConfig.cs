using System.Web.Http;

namespace RP.Enterprise.Foundation.Activity.Service.Logging.Reader
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API routes
            config.MapHttpAttributeRoutes();
        }
    }
}
