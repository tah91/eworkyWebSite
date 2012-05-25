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
            return View(MVC.Widget.Search.Views.Index, new SearchCriteriaFormViewModel(criteria));
        }

        /// <summary>
        /// Ajax POST Action result to search localisations from a SearchCriteria
        /// </summary>
        /// <param name="criteria">The criteria data from the form</param>
        /// <returns>json containing results</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [ValidateOnlyIncomingValues(Exclude = "Type", Prefix = "criteria.OfferData")]
        [HandleModelStateException]
        public virtual ActionResult AjaxSearch(SearchCriteria criteria)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var rvd = _SearchService.GetRVD(criteria);
                    var criteriaViewModel = _SearchService.FillSearchResults(criteria);

                    var listResult = this.RenderRazorViewToString(MVC.Localisation.Views._SearchResults, criteriaViewModel);

                    return Json(new { list = listResult });
                }
                catch (Exception ex)
                {
                    _Logger.Error("AjaxSearch", ex);
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.CheckCriterias);
                    throw new ModelStateException(ModelState);
                }
            }
            throw new ModelStateException(ModelState);
        }

    }
}
