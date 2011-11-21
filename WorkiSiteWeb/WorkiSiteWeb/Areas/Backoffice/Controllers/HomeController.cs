using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Worki.Web.Areas.Backoffice.Controllers
{
    public partial class HomeController : Controller
    {
        //
        // GET: /Backoffice/Home/

        public virtual ActionResult Index()
        {
            return View();
        }

    }
}
