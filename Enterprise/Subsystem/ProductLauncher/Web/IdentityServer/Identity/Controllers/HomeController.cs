using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Base;
using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using RP.Enterprise.Subsystem.ProductLauncher.Web.IdentityHelper.Logic;
using System;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

		/// <summary>
		/// Check to see if the user uses a third party idp
		/// </summary>
		/// <param name="username">The unified login user name</param>
		/// <param name="signin">The sign in token for the client logging in</param>
		/// <returns></returns>
        public ActionResult IdpRedirect(string username, string signin)
        {
            if (string.IsNullOrEmpty(username))
                return null;

	        if (string.IsNullOrEmpty(signin))
		        return null;

			// Get User IDP; 
			var manageProvider = new ManageProvider();
			var identityProviderType = manageProvider.GetProviderByEnterpriseUserName(username);

            if (string.IsNullOrEmpty(identityProviderType.AuthenticationType))
            {
                return null; // user provider not configured in database
            }
            var authenticationManager = HttpContext.GetOwinContext().Authentication;

            if (identityProviderType.AuthenticationType.Equals("ID3", StringComparison.OrdinalIgnoreCase) || identityProviderType.AuthenticationType.Equals("LOCAL", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

	        var returnUrl = ConfigReader.GetIssuerUri + $"/external?provider={identityProviderType.AuthenticationType}&signin={signin}&info={Convert.ToBase64String(Encoding.UTF8.GetBytes(username.ToLower()))}";

			return Json(new { url = returnUrl });
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

        public void Signout()
        {
            var authenticationManager = HttpContext.GetOwinContext().Authentication;
            authenticationManager.SignOut();
        }
    }
}
