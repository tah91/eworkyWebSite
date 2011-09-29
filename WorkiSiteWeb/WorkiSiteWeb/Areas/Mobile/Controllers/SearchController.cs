using System;
using System.Web.Mvc;
using Worki.Infrastructure;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Web.Helpers;
using Worki.Data.Repository;
using Worki.Data.Models;
using System.Linq;
using Worki.Service;

namespace Worki.Web.Areas.Mobile.Controllers
{
    [HandleError]
    [ValidateOnlyIncomingValues]
    public partial class SearchController : Controller
    {
        ILogger _Logger;
        ISearchService _SearchService;

        public SearchController(ILogger logger, ISearchService searchService)
        {
            _Logger = logger;
            _SearchService = searchService;
        }


        /// <summary>
        /// POST Action result to search localisations from a SearchCriteria
        /// it remove the cached result from session store
        /// then it create the route data from SearchCriteria and redirect to result page
        /// the SearchCriteria is pre filled according to the profile (students, teleworker etc...)
        /// </summary>
        /// <param name="criteria">The criteria data from the form</param>
        /// <returns>redirect to the list of results</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("recherche-lieu-travail")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult FullSearch(SearchCriteria criteria)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //Session.Remove(SearchService.CriteriaViewModelKey);
                    var rvd = _SearchService.GetRVD(criteria);
                    return RedirectToAction(MVC.Mobile.Search.Actions.ActionNames.FullSearchResult, rvd);
                }
                catch (Exception ex)
                {
                    _Logger.Error("FullSearch", ex);
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.CheckCriterias);
                }
            }
            return View(new SearchCriteriaFormViewModel(criteria));
        }

        /// <summary>
        /// GET Action result to show paginated search results from a SearchCriteria
        /// if results in cache
        ///     get it
        /// else
        ///     build searchcriteria from url
        ///     with this, search matchings localisations in repository
        ///     fill the session store with computed results
        /// and display results
        /// </summary>
        /// <param name="page">the page to display</param>
        /// <returns>the list of results in the page</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("resultats-liste")]
        public virtual ActionResult FullSearchResult(int? page)
        {
            var pageValue = page ?? 1;
            //SearchCriteriaFormViewModel criteriaViewModel = null;
            var criteria = _SearchService.GetCriteria(Request);
            var criteriaViewModel = _SearchService.FillSearchResults(criteria);

            criteriaViewModel.FillPageInfo(pageValue);
            return View(criteriaViewModel);
        }

        /// <summary>
        /// GET Action result to show detailed localisation from search results
        /// </summary>
        /// <param name="index">the index of th localisation in the list of results</param>
        /// <returns>a view of the details of the selected localisation</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("resultats-detail")]
        public virtual ActionResult FullSearchResultDetail(int? index)
        {
            var itemIndex = index ?? 0;
            var detailModel = _SearchService.GetSingleResult(Request, itemIndex);

            if (detailModel == null)
                return View(MVC.Shared.Views.Error);
            return View(MVC.Mobile.Shared.Views.resultats_detail, detailModel);
        }

    }
}
