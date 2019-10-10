using System.Web.Mvc;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Identity.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult NotFound()
        {
            return View();
        }
    }
}