using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Worki.Web.Areas.Backoffice.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Backoffice/Home/

        public ActionResult Index()
        {
            return View();
        }

    }
}
