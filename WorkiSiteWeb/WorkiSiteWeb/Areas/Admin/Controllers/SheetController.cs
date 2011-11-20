using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Worki.Web.Areas.Admin.Controllers
{
	public partial class SheetController : Controller
    {
        //
        // GET: /Admin/Sheet/

		public virtual ActionResult Index()
        {
            return View();
        }

    }
}
