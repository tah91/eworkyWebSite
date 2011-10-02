using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Data.Models;

namespace Worki.Web.Areas.Mobile.Controllers
{
    public partial class HomeController : Controller
    {
        [ActionName("index")]
        public virtual ActionResult Index()
        {
			return View(new SearchCriteriaFormViewModel());
        }
    }
}
