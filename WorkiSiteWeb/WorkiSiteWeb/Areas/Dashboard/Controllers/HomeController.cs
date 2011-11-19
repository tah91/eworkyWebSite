using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Worki.Web.Areas.Dashboard.Controllers
{
	public partial class HomeController : Controller
    {
        //
        // GET: /Dashboard/Home/

		public virtual ActionResult Index()
        {
            return View();
        }

    }
}
