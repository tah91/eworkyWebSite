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
using Worki.Web.Helpers;

namespace Worki.Web.Areas.Widget.Controllers
{
    public partial class IntermediateController : ControllerBase
    {
        ISearchService _SearchService;

        public IntermediateController(  ILogger logger,
                                        IObjectStore objectStore,
                                        ISearchService searchService)
            : base(logger, objectStore)
        {
            _SearchService = searchService;
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
        /// GET Action result for unavailable finder
        /// </summary>
        /// <returns>view with iframe</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult Error()
        {
            return View();
        }

        /// <summary>
        /// GET Action result for dispatch iframe to correct action
        /// </summary>
        /// <returns>view with iframe</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [PreserveQueryString(ToKeep = MiscHelpers.WidgetConstants.ParamToKeep)]
        public virtual ActionResult Dispatch()
        {
            var appId = Url.GetQueryParam(MiscHelpers.WidgetConstants.AppKey);
            if (string.IsNullOrEmpty(appId))
            {
                _Logger.Error("Dispatch : appid null");
                return RedirectToAction(MVC.Widget.Intermediate.Error());
            }

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);

            var member = mRepo.Get(m => m.MemberMainData.ApiKey == appId);
            if (member == null)
            {
                _Logger.Error("Dispatch : appid " + appId + " not found");
                return RedirectToAction(MVC.Widget.Intermediate.Error());
            }

            var kind=Url.GetQueryParam(MiscHelpers.WidgetConstants.Kind);

            switch (kind)
            {
                case MiscHelpers.WidgetConstants.KindDetail:
                    {
                        int id;
                        if (int.TryParse(Url.GetQueryParam(MiscHelpers.WidgetConstants.ItemId), out id))
                        {
                            return RedirectToAction(MVC.Widget.Localisation.Detail(id));
                        }
                        else
                        {
                            return RedirectToAction(MVC.Widget.Localisation.Index());
                        }
                    }
                case MiscHelpers.WidgetConstants.KindFinderFetched:
                    {
                        //var place = Url.GetQueryParam(MiscHelpers.SeoConstants.Place);
                        //var country = Url.GetQueryParam(MiscHelpers.WidgetConstants.Country);
                        var criteria = _SearchService.GetCriteria(Request);
                        var rvd = _SearchService.GetRVD(criteria);
                        return RedirectToAction(MVC.Widget.Localisation.ActionNames.SearchResult, MVC.Widget.Localisation.Name, rvd);
                    }
                case MiscHelpers.WidgetConstants.KindFinder:
                default:
                    return RedirectToAction(MVC.Widget.Localisation.Index());
            }
        }
    }
}
