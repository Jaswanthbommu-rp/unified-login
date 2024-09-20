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

            var strURLArray = new string[] { };
            String redirect = Request.QueryString["idp"];
            Int32 strDest = System.Convert.ToInt32(redirect);
            if ((strDest >= 0) && (strDest <= strURLArray.Length - 1))
            {
                var idp = strURLArray[strDest];
                if (string.IsNullOrEmpty(idp))
                    throw new Exception("No idp included in redirect querystring!!");

                var returnUrl = ConfigReader.GetRedirectUri.TrimEnd('/') + "?auth=" + idp;
                Response.Redirect(returnUrl, false);

            }
        }
    }
}