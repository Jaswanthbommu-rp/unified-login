using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using System.Web.Mvc;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Controllers
{
    /// <summary>
    /// Used to initiate a login from a third party identity provider that does not require a custom client context
    /// </summary>
    public class AuthController : BaseController
    {
		[AllowAnonymous]
	    public ActionResult AzureConsent(string error)
	    {
            if (string.IsNullOrEmpty(error))
            {
                ViewBag.StatusMessage = "You've successfully connected RealPage to Azure!";
                ViewBag.AdditionalStatusInfo = "You can close this tab now and return to your work.";
                ViewBag.ErrorStatus = false;
            }
            else
            {
                if (error == "access_denied")
                {
                    ViewBag.StatusMessage = "There was an error connecting to Azure.";
                    ViewBag.AdditionalStatusInfo = "Check your Azure permissions and try again.";
                }
                ViewBag.ErrorStatus = true;
            }
            return View();
	    }

        [AllowAnonymous]
        public ActionResult EntraConsent(string error)
        {
            if (string.IsNullOrEmpty(error))
            {
                ViewBag.StatusMessage = "You've successfully connected RealPage to Entra!";
                ViewBag.AdditionalStatusInfo = "You can close this tab now and return to your work.";
                ViewBag.ErrorStatus = false;
            }
            else
            {
                if (error == "access_denied")
                {
                    ViewBag.StatusMessage = "There was an error connecting to Entra.";
                    ViewBag.AdditionalStatusInfo = "Check your Entra permissions and try again.";
                }
                ViewBag.ErrorStatus = true;
            }
            return View();
        }
        /// <summary>
        /// Used to initiate a login from an Azure user
        /// </summary>
        /// <returns></returns>
        public ActionResult Azure()
        {
            var returnUrl = ConfigReader.GetRedirectUri.TrimEnd('/') + "?auth=azure";

            Response.Redirect(returnUrl, false);
			return View();
	    }


	    /// <summary>
	    /// Used to initiate a login from an Azure user without forcing a reauth
	    /// </summary>
	    /// <returns></returns>
	    public ActionResult AzureDirect()
	    {
            var returnUrl = ConfigReader.GetRedirectUri.TrimEnd('/') + "?auth=azuredirect";

            Response.Redirect(returnUrl, false);
		    return View();
	    }

		/// <summary>
		/// Used to initiate a login from a Google user
		/// </summary>
		/// <returns></returns>
		public ActionResult Google()
	    {
            var returnUrl = ConfigReader.GetRedirectUri.TrimEnd('/') + "?auth=google";

            Response.Redirect(returnUrl, false);
		    return View();
	    }

	    /// <summary>
	    /// Used to initiate a login from a Google user without forcing a reauth
	    /// </summary>
	    /// <returns></returns>
	    public ActionResult GoogleDirect()
	    {
            var returnUrl = ConfigReader.GetRedirectUri.TrimEnd('/') + "?auth=googledirect";

            Response.Redirect(returnUrl);
		    return View();
	    }
	}
}