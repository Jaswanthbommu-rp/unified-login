using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RP.Enterprise.Subsystem.ProductLauncher.Web.Landing.Controllers
{
    public class CompaniesController : BaseController
    {
        // GET: Company
        public ActionResult Index()
        {
            return View();
        }
    }
}