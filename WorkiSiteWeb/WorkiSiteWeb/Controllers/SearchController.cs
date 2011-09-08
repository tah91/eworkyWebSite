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
using Worki.Services;

namespace Worki.Web.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
    [ValidateOnlyIncomingValues]
    public partial class SearchController : Controller
    {
        IMemberRepository _MembertRepository;
        ILocalisationRepository _LocalisationRepository;
        ILogger _Logger;
		ISearchService _SearchService;

        public SearchController(ILocalisationRepository localisationRepository,IMemberRepository memberRepository, ILogger logger,ISearchService searchService)
        {
            _LocalisationRepository = localisationRepository;
            _MembertRepository = memberRepository;
            _Logger = logger;
			_SearchService = searchService;
        }

        /// <summary>
        /// POST Action result to get localisations very close to given coordinates
        /// it returns json data
        /// </summary>
        /// <param name="latitude">coordinates latitude</param>
        /// <param name="longitude">coordinates longitude</param>
        /// <returns>list of localisations in json format</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult FindSimilarLocalisation(float latitude, float longitude)
        {
            var localisations = _LocalisationRepository.FindSimilarLocalisation(latitude, longitude);
            var jsonLocalisations = (from item
                                         in localisations
                                     select item.GetJson());
            return Json(jsonLocalisations.ToList());
        }

        /// <summary>
        /// POST Action result to get main localisations
        /// it returns json data
        /// </summary>
        /// <returns>list of localisations in json format</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult GetMainLocalisations()
        {
            var urlHelper = new UrlHelper(ControllerContext.RequestContext);
			var localisations = _LocalisationRepository.GetMany(item => (item.MainLocalisation != null && item.LocalisationFiles.Where(f => f.IsDefault == true).Count() != 0));
            var jsonLocalisations = localisations.Select(item =>
            {
                return item.GetJson(this);
            });
            return Json(jsonLocalisations.ToList());
        }

        /// <summary>
        /// GET Action result to search localisations from a SearchCriteria
        /// the search is per offer (free area, meeting room etc...)
        /// </summary>
        /// <returns>the form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("recherche-lieu-travail")]
        public virtual ActionResult FullSearch()
        {
            var criteria = new SearchCriteria(true);
            return View(new SearchCriteriaFormViewModel(criteria, false, eSearchType.ePerOffer));
        }

        /// <summary>
        /// GET Action result to search localisations from a SearchCriteria
        /// the search is per localisation type (wifi spot, hotel, resto etc...)
        /// </summary>
        /// <returns>the form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("recherche-lieu-travail-menu")]
        public virtual ActionResult FullSearchOffer(int offertID)// Pré selection of the list box of recherche
        {
            var criteria = new SearchCriteria { LocalisationOffer = offertID };
            return View(MVC.Search.Views.recherche_lieu_travail, new SearchCriteriaFormViewModel(criteria, false, eSearchType.ePerOffer));
        }

        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("recherche-lieu-travail-par-type")]
        public virtual ActionResult FullSearchPerType()
        {
            var criteria = new SearchCriteria();
            return View(MVC.Search.Views.recherche_lieu_travail, new SearchCriteriaFormViewModel(criteria, false, eSearchType.ePerType));
        }

        /// <summary>
        /// GET Action result to search localisations from a SearchCriteria
        /// the search is per localisation type (wifi spot, hotel, resto etc...)
        /// the SearchCriteria is pre filled according to the profile (students, teleworker etc...)
        /// </summary>
        /// <returns>the form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("recherche-lieu-travail-special")]
        public virtual ActionResult FullSearchPerTypeSpecial(int type)
        {
            var directAccessType = (eDirectAccessType)type;
            SearchCriteria criteria = null;
            switch (directAccessType)
            {
                case eDirectAccessType.eStudent:
                    criteria = new SearchCriteria { SpotWifi = true, CoffeeResto = true, Biblio = true, PublicSpace = true };
                    break;
                case eDirectAccessType.eTeleworker:
                    criteria = new SearchCriteria { PublicSpace = true, Telecentre = true, BuisnessCenter = true, CoworkingSpace = true };
                    break;
                case eDirectAccessType.eStartUp:
                    criteria = new SearchCriteria { BuisnessCenter = true, CoworkingSpace = true, WorkingHotel = true };
                    break;
                case eDirectAccessType.eNomade:
                    criteria = new SearchCriteria { SpotWifi = true, CoffeeResto = true, Biblio = true, TravelerSpace = true, Hotel = true, Telecentre = true, BuisnessCenter = true, CoworkingSpace = true };
                    break;
                case eDirectAccessType.eEntreprise:
                    criteria = new SearchCriteria { Hotel = true, Telecentre = true, BuisnessCenter = true, WorkingHotel = true, PrivateArea = true };
                    break;
                case eDirectAccessType.eIndependant:
                    criteria = new SearchCriteria { Telecentre = true, BuisnessCenter = true, CoworkingSpace = true, WorkingHotel = true };
                    break;
            }
            criteria.Everything = false;
            return View(MVC.Search.Views.recherche_lieu_travail, new SearchCriteriaFormViewModel(criteria, false, eSearchType.ePerType, directAccessType));
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
                    return RedirectToAction(MVC.Search.Actions.ActionNames.FullSearchResult, rvd);
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

			if (detailModel==null)
				return View(MVC.Shared.Views.Error);
			return View(MVC.Shared.Views.resultats_detail, detailModel);
		}
    }
}
