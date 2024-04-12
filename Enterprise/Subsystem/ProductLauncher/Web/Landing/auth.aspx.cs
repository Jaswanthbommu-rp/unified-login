using RP.Enterprise.Subsystem.ProductLauncher.Component.SharedObjects.Helper;
using System;
using System.Collections.Generic;
using System.Web;
using Thinktecture.IdentityModel.Client;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing
{
	public partial class auth : System.Web.UI.Page
    {
		
        protected void Page_Load(object sender, EventArgs e)
        {
			Response.Cache.SetCacheability(HttpCacheability.NoCache);
	        Response.AppendHeader("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
	        Response.AppendHeader("Pragma", "no-cache"); // HTTP 1.0.
	        Response.AppendHeader("Expires", "0"); // Proxies.

			var idp = Request.QueryString["idp"];
            var tok = Request["id_token"];
            var atok = Request["access_token"];
            if (string.IsNullOrEmpty(idp))
                throw new Exception("No idp included in redirect querystring!!");

            var scopesForAuth = ConfigReader.GetLandingScopes;
            var state = Guid.NewGuid().ToString("N");
            var nonce = Guid.NewGuid().ToString("N");

            var client = new OAuth2Client(new Uri(ConfigReader.GetIssuerUri + "/connect/authorize"));
	        Dictionary<string, string> additional = new Dictionary<string, string>();
	        additional.Add("prompt", "login");

			var returnUrlForOkta = client.CreateAuthorizeUrl(ConfigReader.GetLandingClientId, "id_token token", scopesForAuth, ConfigReader.GetRedirectUri,
                state, nonce, acrValues: string.Format("idp:{0}", idp), responseMode: "form_post", additionalValues: additional);

            Response.Redirect(returnUrlForOkta, false);

        }
    }
}