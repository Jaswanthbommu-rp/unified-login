using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Controllers
{
	[Authorize]
    public class SettingController : BaseController
    {

        // GET: Settings
        public ActionResult Index()
        {
            //HttpContext.Request.QueryString;
        
            string OrganizationMasterId = GetUserMasterId();
            ViewBag.OrganizationMasterId = OrganizationMasterId;
            ViewBag.UnifiedSettingViewRights = IsUserAllowedtoViewData();
            ViewBag.UnifiedSettingManageRights = IsUserAllowedtoManageData();

            if (HttpContext.Request.QueryString["companyID"] != null)
            {
                string qsCompanyId = HttpContext.Request.QueryString["companyID"];
                if ( OrganizationMasterId != qsCompanyId && OrganizationMasterId != "-1")
                {
                    return File(Server.MapPath("settings/error/access-denied/") + "index.html", "text/html");
                }
            }
            return View();
        }

        private static string GetUserMasterId()
        {
            string OrganizationMasterId = "";
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (currentClaimPrincipal.Identity.IsAuthenticated)
            {
                OrganizationMasterId = ((from nvp in currentClaimPrincipal.Claims where nvp.Type.ToUpper() == "ORGMASTERID" select nvp.Value).FirstOrDefault());
            }

            return OrganizationMasterId;
        }

        private static bool IsUserAllowedtoViewData()
        {
            string strRights = string.Empty;
            bool isUserAllowedtoView = false;
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (currentClaimPrincipal.Identity.IsAuthenticated)
            {
                strRights = ((from nvp in currentClaimPrincipal.Claims where nvp.Type.ToUpper() == "RIGHT" && nvp.Value.ToUpper() == "VIEWUNIFIEDSETTINGS" select nvp.Value).FirstOrDefault());
                if (!string.IsNullOrEmpty(strRights))
                    isUserAllowedtoView = true;
            }

            return isUserAllowedtoView;
        }

        private static bool IsUserAllowedtoManageData()
        {
            string strRights = string.Empty;
            bool isUserAllowedtoManage = false;
            ClaimsPrincipal currentClaimPrincipal = ClaimsPrincipal.Current;
            if (currentClaimPrincipal.Identity.IsAuthenticated)
            {
                strRights = ((from nvp in currentClaimPrincipal.Claims where nvp.Type.ToUpper() == "RIGHT" && nvp.Value.ToUpper() == "MANAGEUNIFIEDSETTINGS" select nvp.Value).FirstOrDefault());
                if (!string.IsNullOrEmpty(strRights))
                    isUserAllowedtoManage = true;
            }

            return isUserAllowedtoManage;
        }

        public ActionResult SystemSecurity()
        {

            string OrganizationMasterId = GetUserMasterId();
            ViewBag.OrganizationMasterId = OrganizationMasterId;
            ViewBag.UnifiedSettingViewRights = IsUserAllowedtoViewData();
            ViewBag.UnifiedSettingManageRights = IsUserAllowedtoManageData();

			if (HttpContext.Request.QueryString["companyID"] != null)
			{
				string qsCompanyId = HttpContext.Request.QueryString["companyID"];
				if (OrganizationMasterId != qsCompanyId && OrganizationMasterId != "-1")
				{
					return File(Server.MapPath("settings/error/access-denied/") + "index.html", "text/html");
				}
			}
			return View();
        }

        public ActionResult CompanySettings()
        {
            string OrganizationMasterId = GetUserMasterId();
            ViewBag.OrganizationMasterId = OrganizationMasterId;
            ViewBag.UnifiedSettingViewRights = IsUserAllowedtoViewData();
            ViewBag.UnifiedSettingManageRights = IsUserAllowedtoManageData();

            if (HttpContext.Request.QueryString["companyID"] != null)
            {
                string qsCompanyId = HttpContext.Request.QueryString["companyID"];
                if (OrganizationMasterId != qsCompanyId && OrganizationMasterId != "-1")
                {
                    return File(Server.MapPath("settings/error/access-denied/") + "index.html", "text/html");
                }
            }
            return View();
        }

        //For Simon Learn Help Page

        public ActionResult TrainingAndHelp()
        {
            string OrganizationMasterId = GetUserMasterId();
            ViewBag.OrganizationMasterId = OrganizationMasterId;
            ViewBag.UnifiedSettingViewRights = IsUserAllowedtoViewData();
            ViewBag.UnifiedSettingManageRights = IsUserAllowedtoManageData();
            return View();
        }

		/*public ActionResult DynamicSettings()
		{
			string OrganizationMasterId = GetUserMasterId();
			ViewBag.OrganizationMasterId = OrganizationMasterId;
			ViewBag.UnifiedSettingViewRights = IsUserAllowedtoViewData();
			ViewBag.UnifiedSettingManageRights = IsUserAllowedtoManageData();
			return View();
		}*/

		/*public ActionResult SearchResults()
		{
			string OrganizationMasterId = GetUserMasterId();
			ViewBag.OrganizationMasterId = OrganizationMasterId;
			ViewBag.UnifiedSettingViewRights = IsUserAllowedtoViewData();
			ViewBag.UnifiedSettingManageRights = IsUserAllowedtoManageData();

			if (HttpContext.Request.QueryString["companyID"] != null)
			{
				string qsCompanyId = HttpContext.Request.QueryString["companyID"];
				if (OrganizationMasterId != qsCompanyId && OrganizationMasterId != "-1")
				{
					return File(Server.MapPath("/settings/error/access-denied/") + "index.html", "text/html");
				}
			}
			return View();
		}*/

	}
}