using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure;
using Worki.Service;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;

namespace Worki.Web.Areas.Widget.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    [DontRequireHttps]
    public abstract class ControllerBase : Controller
    {
        protected ILogger _Logger;
        protected IObjectStore _ObjectStore;

        public ControllerBase()
        {
        }

        public ControllerBase(ILogger logger, IObjectStore objectStore)
        {
            this._Logger = logger;
            this._ObjectStore = objectStore;
        }
    }

    public partial class SearchController : ControllerBase
    {
        ISearchService _SearchService;

        public SearchController(ILogger logger, IObjectStore objectStore, ISearchService searchService)
            : base(logger, objectStore)
        {
            _SearchService = searchService;
        }

        /// <summary>
        /// GET Action result to search localisations from a SearchCriteria
        /// the search is per offer (free area, meeting room etc...)
        /// </summary>
        /// <returns>the form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult Index()
        {
            var criteria = new SearchCriteria(true);
            return View(new SearchCriteriaFormViewModel(criteria));
        }

        /// <summary>
        /// Ajax POST Action result to search localisations from a SearchCriteria
        /// </summary>
        /// <param name="criteria">The criteria data from the form</param>
        /// <returns>redirect to results</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [ValidateOnlyIncomingValues(Exclude = "Type", Prefix = "criteria.OfferData")]
        public virtual ActionResult Search(SearchCriteria criteria)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var rvd = _SearchService.GetRVD(criteria);
                    return RedirectToAction(MVC.Widget.Search.ActionNames.SearchResult, rvd);
                }
                catch (Exception ex)
                {
                    _Logger.Error("Search", ex);
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.CheckCriterias);
                }
            }
            return View(MVC.Widget.Search.Views.Index, new SearchCriteriaFormViewModel(criteria));
        }

        /// <summary>
        /// GET Action result to show paginated search results from a SearchCriteria
        /// </summary>
        /// <param name="page">the page to display</param>
        /// <returns>the list of results in the page</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult SearchResult(int? page)
        {
            var pageValue = page ?? 1;
            var criteria = _SearchService.GetCriteria(Request);
            var criteriaViewModel = _SearchService.FillSearchResults(criteria);

            criteriaViewModel.FillPageInfo(pageValue);
            return View(criteriaViewModel);
        }

    }
}
