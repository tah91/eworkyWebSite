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
using Worki.Infrastructure.Helpers;

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

    [PreserveQueryString(ToKeep = MiscHelpers.WidgetConstants.ParamToKeep)]
    public partial class LocalisationController : ControllerBase
    {
        ISearchService _SearchService;
        int _PageSize = 6;

        public LocalisationController(  ILogger logger,
                                        IObjectStore objectStore,
                                        ISearchService searchService)
            : base(logger, objectStore)
        {
            _SearchService = searchService;
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

            //var detailModel = new SearchSingleResultViewModel { Localisation = localisation };
            return View(MVC.Widget.Localisation.Views.Detail, localisation);
        }

        /// <summary>
        /// GET Action result to search localisations from a SearchCriteria
        /// the search is per offer (free area, meeting room etc...)
        /// </summary>
        /// <returns>the form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult Index()
        {
            var criteria = _SearchService.GetCriteria(Request);
            return View(new SearchCriteriaFormViewModel(criteria));
        }

        /// <summary>
        /// Ajax POST Action result to search localisations from a SearchCriteria
        /// </summary>
        /// <param name="criteria">The criteria data from the form</param>
        /// <returns>redirect to results</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        //[ValidateAntiForgeryToken]
        [ValidateOnlyIncomingValues(Exclude = "Type", Prefix = "criteria.OfferData")]
        public virtual ActionResult Search(SearchCriteria criteria)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var rvd = _SearchService.GetRVD(criteria);
                    return RedirectToAction(MVC.Widget.Localisation.ActionNames.SearchResult, rvd);
                }
                catch (Exception ex)
                {
                    _Logger.Error("Search", ex);
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.CheckCriterias);
                }
            }
            return View(MVC.Widget.Localisation.Views.Index, new SearchCriteriaFormViewModel(criteria));
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

            criteriaViewModel.FillPageInfo(pageValue, _PageSize);
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
            return View(MVC.Widget.Localisation.Views.Detail, detailModel.Localisation);
        }

        #region Ajax Search

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
                        var listResult = this.RenderRazorViewToString(MVC.Widget.Localisation.Views._Results, criteriaViewModel);
                        var locList = (from item in criteriaViewModel.PageResults select item.GetJson());
                        var dict = criteriaViewModel.Criteria.GetDictionnary();
                        var rvd = new RouteValueDictionary(dict);
                        var link = Url.Action(MVC.Widget.Localisation.ActionNames.SearchResult, MVC.Widget.Localisation.Name, rvd);
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
        //[ValidateAntiForgeryToken]
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

            criteriaViewModel.FillPageInfo(pageValue, _PageSize);
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

            return PartialView(MVC.Widget.Localisation.Views._MapItemSummary, localisation);
        }

        #endregion

        #region Booking

        /// <summary>
        /// GET Action result to show booking form
        /// </summary>
        /// <param name="id">id of offer to book</param>
        /// <returns>View containing booking form</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult CreateBooking(int id)
        {
            var memberId = WebHelper.GetIdentityId(User.Identity);

            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var offer = oRepo.Get(id);
            var member = mRepo.Get(memberId);

            var formModel = new MemberBookingFormViewModel(member, offer);

            return PartialView(MVC.Widget.Localisation.Views._CreateBooking, formModel);
        }

        /// <summary>
        /// Post Action result to add booking request
        /// </summary>
        /// <returns>View containing booking form</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [HandleModelStateException]
        public virtual ActionResult CreateBooking(int id, MemberBookingFormViewModel formData)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var memberId = WebHelper.GetIdentityId(User.Identity);
            var member = mRepo.Get(memberId);
            var offer = oRepo.Get(id);

            if (ModelState.IsValid)
            {
                try
                {
                    member = mRepo.Get(memberId);

                    var locName = offer.Localisation.Name;
                    var locUrl = offer.Localisation.GetDetailFullUrl(Url);
                    try
                    {
                        formData.MemberBooking.MemberId = memberId;
                        formData.MemberBooking.OfferId = id;
                        formData.MemberBooking.StatusId = (int)MemberBooking.Status.Unknown;
                        formData.AjustBookingPeriod();
                        formData.MemberBooking.Price = offer.GetDefaultPrice(formData.MemberBooking.FromDate,
                                                                                formData.MemberBooking.ToDate,
                                                                                formData.MemberBooking.PeriodType == (int)MemberBooking.ePeriodType.SpendUnit,
                                                                                (Offer.PaymentPeriod)formData.MemberBooking.TimeType,
                                                                                formData.MemberBooking.TimeUnits);
                        //set phone number to the one from form
                        member.MemberMainData.PhoneNumber = formData.PhoneNumber;
                        member.MemberBookings.Add(formData.MemberBooking);

                        formData.MemberBooking.MemberBookingLogs.Add(new MemberBookingLog
                        {
                            CreatedDate = DateTime.UtcNow,
                            Event = "Booking Created",
                            EventType = (int)MemberBookingLog.BookingEvent.Creation,
                            LoggerId = memberId
                        });

                        formData.MemberBooking.InvoiceNumber = new InvoiceNumber();

                        if (!offer.Localisation.HasClient(memberId))
                        {
                            offer.Localisation.LocalisationClients.Add(new LocalisationClient { ClientId = memberId });
                        }

                        //send mail to team
                        dynamic teamMail = new Email(MVC.Emails.Views.Email);
                        teamMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                        teamMail.To = MiscHelpers.EmailConstants.BookingMail;
                        teamMail.Subject = Worki.Resources.Email.BookingString.BookingMailSubject;
                        teamMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
                        teamMail.Content = string.Format(Worki.Resources.Email.BookingString.CreateBookingTeam,
                                                         string.Format("{0} {1}", member.MemberMainData.FirstName, member.MemberMainData.LastName),
                                                         formData.PhoneNumber,
                                                         member.Email,
                                                         locName,
                                                         Localisation.GetOfferType(offer.Type),
                                                         formData.MemberBooking.GetStartDate(),
                                                         formData.MemberBooking.GetEndDate(),
                                                         formData.MemberBooking.Message,
                                                         locUrl);

                        //send mail to booking member
                        dynamic clientMail = new Email(MVC.Emails.Views.Email);
                        clientMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                        clientMail.To = member.Email;
                        clientMail.Subject = Worki.Resources.Email.BookingString.CreateBookingClientSubject;
                        clientMail.ToName = member.MemberMainData.FirstName;
                        clientMail.Content = string.Format(Worki.Resources.Email.BookingString.CreateBookingClient,
                                                         Localisation.GetOfferType(offer.Type),
                                                         formData.MemberBooking.GetStartDate(),
                                                         formData.MemberBooking.GetEndDate(),
                                                         locName,
                                                         offer.Localisation.Adress);

                        //send mail to localisation member
                        var urlHelp = new UrlHelper(ControllerContext.RequestContext);
                        var ownerUrl = urlHelp.ActionAbsolute(MVC.Backoffice.Home.Booking());
                        TagBuilder ownerLink = new TagBuilder("a");
                        ownerLink.MergeAttribute("href", ownerUrl);
                        ownerLink.InnerHtml = Worki.Resources.Views.Account.AccountString.OwnerSpace;

                        dynamic ownerMail = new Email(MVC.Emails.Views.Email);
                        ownerMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                        ownerMail.To = offer.Localisation.Member.Email;
                        ownerMail.Subject = string.Format(Worki.Resources.Email.BookingString.BookingOwnerSubject, locName);
                        ownerMail.ToName = offer.Localisation.Member.MemberMainData.FirstName;
                        ownerMail.Content = string.Format(Worki.Resources.Email.BookingString.BookingOwnerBody,
                                                        Localisation.GetOfferType(offer.Type),
                                                        locName,
                                                        offer.Localisation.Adress,
                                                        ownerLink);

                        context.Commit();

                        clientMail.Send();
                        teamMail.Send();
                        ownerMail.Send();
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }

                    TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Booking.BookingString.Confirmed;
                    return Json(Url.RequestContext.HttpContext.Request.UrlReferrer);
                }
                catch (Exception ex)
                {
                    _Logger.Error("Create", ex);
                    ModelState.AddModelError("", ex.Message);
                }
            }
            throw new ModelStateException(ModelState);
        }

        #endregion
    }
}
