namespace UnifiedLogin.SharedObjects.Swagger
{
	/// <summary>
	/// swagger api
	/// </summary>
    public class SwaggerUtil
    {
        /// <summary>
        /// Used to get the url for the swagger api document.
        /// </summary>
        /// <param name="request">The http request being processed</param>
        /// <param name="routePath">The path to the webapi route being documented</param>
        /// <returns></returns>
        public string GetUrl(System.Net.Http.HttpRequestMessage request, string routePath)
        {
            string rootUrl = (request.RequestUri.Host.ToLower().Contains("localhost") ? "http://" : "https://") + request.RequestUri.Host.ToLower() + routePath;
            return rootUrl;
        }
    }
}
