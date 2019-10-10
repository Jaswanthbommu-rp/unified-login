using Microsoft.Owin;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using System;
using System.Web;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Client;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Controllers
{
	//[RequireHttps]
	public class HomeController : Controller
	{

		/// <summary>
		/// Default home page for the product
		/// </summary>
		[AllowAnonymous]
		public ActionResult Index()
        {
			RemoveDuplicateToken();

			if (Request.Url.AbsoluteUri.Contains("msgId"))
            {
                string redirectUrl = RemoveQueryStringByKey(Request.Url.AbsoluteUri, "msgId");
                Response.Redirect(redirectUrl);
            }

			return File(System.Web.Hosting.HostingEnvironment.MapPath("~/") + "index.html", "text/html");
        }
		
        private void RemoveDuplicateToken()
        {
			// If the page request gets this far then the user should be authenticated. If the msgId exists in the query string then remove it and redirect back.
	        HttpCookieCollection cookies = Request.Cookies;
	        int accessTokenCount = 0;
	        for (int i = 0; i < cookies.Count; i++)
	        {
		        HttpCookie cook = cookies[i];
		        if (cook.Name.Equals("access_token", StringComparison.OrdinalIgnoreCase))
		        {
			        accessTokenCount++;
		        }
	        }

	        if (accessTokenCount > 1)
	        {
		        var oWinResponse = HttpContext.GetOwinContext().Response;
		        HttpContext.GetOwinContext().Response.OnSendingHeaders(h => { oWinResponse.Cookies.Delete("access_token", new CookieOptions() {Path = "/", Domain = "realpage.com"}); }, null);
	        }
        }

        /// <summary>
        /// Used to sign out the user
        /// </summary>
        public ActionResult Signout(string msgId)
        {
			Response.Redirect(ConfigReader.GetRedirectUri + "logout");
			return null;
        }

		/// <summary>
		/// Used to clear the cache
		/// </summary>
		/// <returns></returns>
		public ActionResult BustCache()
        {
            RPObjectCache cache = new RPObjectCache();
            cache.BustCache();

            return null;
        }

        private static string RemoveQueryStringByKey(string url, string key)
        {
            var uri = new Uri(url);

            // this gets all the query string key value pairs as a collection
            var newQueryString = HttpUtility.ParseQueryString(uri.Query);

            // this removes the key if exists
            newQueryString.Remove(key);

            // this gets the page path from root without QueryString
            string pagePathWithoutQueryString = uri.GetLeftPart(UriPartial.Path);

            return newQueryString.Count > 0
                ? String.Format("{0}?{1}", pagePathWithoutQueryString, newQueryString)
                : pagePathWithoutQueryString;
        }

        // commenetd - might need for single sign out
        //public void SignoutCleanup(string sid)
        //{
        //    var cp = (ClaimsPrincipal)User;
        //    var sidClaim = cp.FindFirst("sid");
        //    if (sidClaim != null && sidClaim.Value == sid)
        //    {
        //        Request.GetOwinContext().Authentication.SignOut("Cookies");
        //    }
        //}
    }
}