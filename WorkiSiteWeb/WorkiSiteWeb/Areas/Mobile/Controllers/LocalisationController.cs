﻿using System;
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
	[CompressFilter(Order = 1)]
	[CacheFilter(Order = 2)]
    public partial class LocalisationController : Controller
    {
        ILogger _Logger;
        ISearchService _SearchService;

		public LocalisationController(ILogger logger, ISearchService searchService)
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
		[ActionName("search")]
		[ValidateAntiForgeryToken]
        [ValidateOnlyIncomingValues(Exclude = "Type", Prefix = "criteria.OfferData")]
		public virtual ActionResult FullSearch(SearchCriteria criteria)
		{
			if (ModelState.IsValid)
			{
				try
				{
					var rvd = _SearchService.GetRVD(criteria);
					return RedirectToAction(MVC.Localisation.Actions.ActionNames.FullSearchResult, rvd);
				}
				catch (Exception ex)
				{
					_Logger.Error("FullSearch", ex);
					ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.CheckCriterias);
				}
			}
            return View(MVC.Mobile.Home.Views.index, new SearchCriteriaFormViewModel(criteria));
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
		[ActionName("search-results")]
		public virtual ActionResult FullSearchResult(int? page)
		{
			var pageValue = page ?? 1;
            var criteria = _SearchService.GetCriteria(Request, pageValue);
			var criteriaViewModel = _SearchService.FillSearchResults(criteria);

			criteriaViewModel.FillPageInfo(pageValue, 10);
			return View(MVC.Mobile.Localisation.Views.FullSearchResult, criteriaViewModel);
		}

		/// <summary>
		/// GET Action result to show detailed localisation from search results
		/// </summary>
		/// <param name="index">the index of th localisation in the list of results</param>
		/// <returns>a view of the details of the selected localisation</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		[ActionName("search-detail")]
		public virtual ActionResult FullSearchResultDetail(int? index)
		{
			var itemIndex = index ?? 0;
			var detailModel = _SearchService.GetSingleResult(Request, itemIndex);

			if (detailModel == null)
				return View(MVC.Shared.Views.Error);
			return View(MVC.Mobile.Localisation.Views.FullSearchResultDetail, detailModel);
		}

		/// <summary>
		/// Action to get localisation detail
		/// </summary>
		/// <param name="id">Id of the localisation</param>
		/// <returns>Localisation Detail</returns>
		public virtual ActionResult LocalisationDetail(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);

			var localisation = lRepo.Get(id);
			if (localisation == null)
				return null;

			var model = new SearchSingleResultViewModel { Localisation = localisation };
			return View(MVC.Mobile.Localisation.Views.FullSearchResultDetail, model);
		}

		/// <summary>
		/// Action to get localisation description
		/// </summary>
		/// <param name="id">Id of the localisation</param>
		/// <returns>Redirect to returnUrl</returns>
		public virtual PartialViewResult LocalisationDescription(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);

			var localisation = lRepo.Get(id);
			if (localisation == null)
				return null;

			var model = new SearchSingleResultViewModel { Localisation = localisation, Index = -1 };
			return PartialView(MVC.Mobile.Localisation.Views._SearchResultSummary, model);
		}
    }
}
