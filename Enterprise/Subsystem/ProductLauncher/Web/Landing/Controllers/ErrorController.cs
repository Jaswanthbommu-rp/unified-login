using System.Web.Mvc;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Controllers
{
    public class ErrorController : BaseController
    {
        public ActionResult Index()
        {
			string sessionID = "00000001";
			if ( Session?.SessionID != null)
			{
				sessionID = Session.SessionID;
			}
			ViewBag.sessionID = sessionID;
            return View();
        }

		public ActionResult ReadOnly()
		{
			ViewBag.readOnly = true;
			return View("Index");
		}

		public ActionResult NotFound()
        {
            return View();
        }
    }
}