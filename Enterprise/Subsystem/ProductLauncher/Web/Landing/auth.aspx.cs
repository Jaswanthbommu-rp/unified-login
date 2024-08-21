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
            if (string.IsNullOrEmpty(idp))
                throw new Exception("No idp included in redirect querystring!!");

            var returnUrl = ConfigReader.GetRedirectUri.TrimEnd('/') + "?auth=" + idp;
            Response.Redirect(returnUrl, false);

        }
    }
}