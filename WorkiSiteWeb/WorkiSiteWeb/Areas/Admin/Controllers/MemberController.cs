using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Worki.Web.Areas.Admin.Controllers
{
	public partial class MemberController : Controller
    {
        //
        // GET: /Admin/Member/

		public virtual ActionResult Index()
        {
            return View();
        }

    }
}
