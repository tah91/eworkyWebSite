using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Service;
using Worki.Memberships;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure;
using Worki.Data.Models;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.Helpers;

namespace Worki.Web.Areas.Widget.Controllers
{
    public partial class IntermediateController : ControllerBase
    {
        public IntermediateController(  ILogger logger,
                                        IObjectStore objectStore)
            : base(logger, objectStore)
        {

        }

        /// <summary>
        /// GET Action result for intermediate iframe
        /// </summary>
        /// <returns>view with iframe</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// GET Action result for dispatch iframe to correct action
        /// </summary>
        /// <returns>view with iframe</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [PreserveQueryString(ToKeep = MiscHelpers.WidgetConstants.ParamToKeep)]
        public virtual ActionResult Dispatch(string kind)
        {
            switch (kind)
            {
                case "detail":
                    {
                        int id;
                        if (int.TryParse(Request.QueryString["item-id"], out id))
                        {
                            return RedirectToAction(MVC.Widget.Localisation.Detail(id));
                        }
                        else
                        {
                            return RedirectToAction(MVC.Widget.Localisation.Index());
                        }
                    }
                case "finder":
                default:
                    return RedirectToAction(MVC.Widget.Localisation.Index());
            }
        }
    }
}
