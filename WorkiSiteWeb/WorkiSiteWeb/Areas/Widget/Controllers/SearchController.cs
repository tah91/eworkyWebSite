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
using Worki.Infrastructure.Repository;
using System.Web.Routing;
using Worki.Memberships;

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
        IFormsAuthenticationService _FormsService;
        IMembershipService _MembershipService;

        public SearchController(ILogger logger,
                                IObjectStore objectStore, 
                                ISearchService searchService,
                                IFormsAuthenticationService formsService, 
                                IMembershipService membershipService)
            : base(logger, objectStore)
        {
            _SearchService = searchService;
           _FormsService = formsService;
           _MembershipService = membershipService;
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

        JsonResult GetSearchResult(SearchCriteriaFormViewModel criteriaViewModel)
        {
            switch (criteriaViewModel.Criteria.ResultView)
            {
                case eResultView.Map:
                    {
                        var locList = (from item in criteriaViewModel.Criteria.Projection select item.GetJson());
                        return Json(new { localisations = locList, place = criteriaViewModel.Criteria.Place }, JsonRequestBehavior.AllowGet);
                    }
                case eResultView.List:
                default:
                    {
                        var listResult = this.RenderRazorViewToString(MVC.Widget.Search.Views._Results, criteriaViewModel);
                        var locList = (from item in criteriaViewModel.PageResults select item.GetJson());
                        var dict = criteriaViewModel.Criteria.GetDictionnary();
                        var rvd = new RouteValueDictionary(dict);
                        var link = Url.Action(MVC.Widget.Search.ActionNames.SearchResult, MVC.Widget.Search.Name, rvd);
                        return Json(new { list = listResult, localisations = locList, place = criteriaViewModel.Criteria.Place, link = link }, JsonRequestBehavior.AllowGet);
                    }
            }

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
                    var criteriaViewModel = _SearchService.FillSearchResults(criteria);
                    return GetSearchResult(criteriaViewModel);
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

        /// <summary>
        /// Ajax GET Action result to show paginated search results from a SearchCriteria
        /// </summary>
        /// <param name="page">the page to display</param>
        /// <returns>JSON of the list of results in the page</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult AjaxSearchResult(int? page)
        {
            var pageValue = page ?? 1;
            var criteria = _SearchService.GetCriteria(Request, pageValue);
            var criteriaViewModel = _SearchService.FillSearchResults(criteria);

            criteriaViewModel.FillPageInfo(pageValue);
            return GetSearchResult(criteriaViewModel);
        }

        /// <summary>
        /// Action to get localisation description
        /// </summary>
        /// <param name="id">Id of the localisation</param>
        /// <returns>Redirect to returnUrl</returns>
        public virtual PartialViewResult MapItemSummary(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);

            var localisation = lRepo.Get(id);
            if (localisation == null)
                return null;

            return PartialView(MVC.Widget.Search.Views._MapItemSummary, localisation);
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
            var criteria = _SearchService.GetCriteria(Request, pageValue);
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
        public virtual ActionResult SearchResultDetail(int? index)
        {
            var itemIndex = index ?? 0;
            var detailModel = _SearchService.GetSingleResult(Request, itemIndex);

            if (detailModel == null)
                return View(MVC.Shared.Views.Error);
            return View(MVC.Widget.Search.Views.Detail, detailModel);
        }

        /// <summary>
        /// GET Action result to show detailed localisation
        /// </summary>
        /// <param name="index">the index of th localisation in the list of results</param>
        /// <returns>a view of the details of the selected localisation</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult Detail(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var localisation = lRepo.Get(id);

            var detailModel = new SearchSingleResultViewModel { Localisation = localisation };
            return View(MVC.Widget.Search.Views.Detail, detailModel);
        }

        public virtual ActionResult LogOn()
        {
            return PartialView(MVC.Widget.Shared.Views._Login, new LogOnModel());
        }

        /// <summary>
        /// POST action method to login to an account, add cookie for display name
        /// </summary>
        /// <param name="model">The logon data from the form</param>
        /// <param name="returnUrl">The url to redirect to in case of sucess</param>
        /// <returns>Redirect to return url if any, if not to home page</returns>
        [HttpPost]
        [HandleModelStateException]
        public virtual ActionResult LogOn(LogOnModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (_MembershipService.ValidateUser(model.Login, model.Password))
                    {
                        var context = ModelFactory.GetUnitOfWork();
                        var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
                        var member = mRepo.GetMember(model.Login);
                        var userData = member.GetUserData();
                        _FormsService.SignIn(model.Login, userData, /*model.RememberMe*/true, ControllerContext.HttpContext.Response);

                        return Json(Url.RequestContext.HttpContext.Request.UrlReferrer);
                    }
                    else
                    {
                        ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.MailOrPasswordNotCorrect);
                    }
                }
                catch (Member.ValidationException ex)
                {
                    _Logger.Error("LogOn", ex);
                    ModelState.AddModelError("", ex.Message);
                }
                catch (Exception ex)
                {
                    _Logger.Error("LogOn", ex);
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.MailOrPasswordNotCorrect);
                }
            }
            throw new ModelStateException(ModelState);
        }

        const string MemberDisplayNameString = "MemberDisplayName";

        public virtual ActionResult LogOff()
        {
            _FormsService.SignOut();
            if (this.ControllerContext.HttpContext.Request.Cookies.AllKeys.Contains(MemberDisplayNameString))
            {
                HttpCookie cookie = ControllerContext.HttpContext.Request.Cookies[MemberDisplayNameString];
                cookie.Expires = DateTime.UtcNow.AddDays(-1);
                this.ControllerContext.HttpContext.Response.Cookies.Add(cookie);
            }
            return RedirectToAction(MVC.Widget.Search.Index());
        }
    }
}
