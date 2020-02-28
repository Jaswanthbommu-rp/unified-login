using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Thinktecture.IdentityModel.Client;

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

		/// <summary>
		/// Used to initiate a login from an Azure user
		/// </summary>
		/// <returns></returns>
		public ActionResult Azure()
		{
			var returnUrl = BuildIDPRequest("aad", true);

            Response.Redirect(returnUrl, false);
			return View();
	    }


	    /// <summary>
	    /// Used to initiate a login from an Azure user without forcing a reauth
	    /// </summary>
	    /// <returns></returns>
	    public ActionResult AzureDirect()
	    {
		    var returnUrl = BuildIDPRequest("aad", false);

		    Response.Redirect(returnUrl, false);
		    return View();
	    }

		/// <summary>
		/// Used to initiate a login from a Google user
		/// </summary>
		/// <returns></returns>
		public ActionResult Google()
	    {
			var returnUrl = BuildIDPRequest("oidcgoogle", true);

		    Response.Redirect(returnUrl, false);
		    return View();
	    }

	    /// <summary>
	    /// Used to initiate a login from a Google user without forcing a reauth
	    /// </summary>
	    /// <returns></returns>
	    public ActionResult GoogleDirect()
	    {
		    var returnUrl = BuildIDPRequest("oidcgoogle", false);

		    Response.Redirect(returnUrl);
		    return View();
	    }

		/// <summary>
		/// Build the request to the third party idp
		/// </summary>
		/// <param name="idp"></param>
		/// <param name="forceLogin">request a forced user login if needed</param>
		/// <returns></returns>
		private static string BuildIDPRequest(string idp, bool forceLogin)
	    {
		    var state = Guid.NewGuid().ToString("N");
		    var nonce = Guid.NewGuid().ToString("N");
		    var client = new OAuth2Client(new Uri($"{ConfigReader.GetIssuerUri}/connect/authorize"));
		    string acrAdditional = "";
		    Dictionary<string, string> additional = null;
			if (forceLogin)
		    {
			    acrAdditional = " prompt:login";
			    additional = new Dictionary<string, string>();
			    additional.Add("prompt", "login");
			}

		    // NOTE! the IDP value is currently case sensitive!
		    var returnUrl = client.CreateAuthorizeUrl(ConfigReader.GetLandingClientId, "id_token token", ConfigReader.GetLandingScopes,
			                    ConfigReader.GetRedirectUri, state, nonce, acrValues: $"idp:{idp}"+ acrAdditional, responseMode: "form_post", additionalValues: additional);
			return returnUrl;
	    }
	}
}