using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using Worki.Data.Models;
using Worki.Infrastructure;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Web.Helpers;
using Worki.Service;
using Worki.Infrastructure.Repository;
using Postal;
using System.Collections.Generic;
using Facebook;

namespace Worki.Web.Controllers
{
	public partial class LocalisationController : ControllerBase
    {
		ISearchService _SearchService;

        public LocalisationController(ILogger logger, IObjectStore objectStore, ISearchService searchService)
            : base(logger, objectStore)
        {
            _SearchService = searchService;
        }

        /// <summary>
        /// Deprecated
        /// </summary>
        /// <returns>The action result.</returns>
        public virtual ActionResult Index(int? page)
        {
			var viewModel = new PagingList<Localisation>();
            return View(viewModel);
        }

        /// <summary>
        /// The view containing the details of a localisation
        /// </summary>
        /// <returns>The action result.</returns>
		public virtual ActionResult DetailsOld(int id, string name)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var localisation = lRepo.Get(id);

            return RedirectPermanent(localisation.GetDetailFullUrl(Url));
		}

        /// <summary>
        /// The view containing the details of a localisation
        /// </summary>
        /// <returns>The action result.</returns>
        [ActionName("details")]
        public virtual ActionResult Details(int id, string name)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var localisation = lRepo.Get(id);
            var nameToMatch = MiscHelpers.GetSeoString(localisation.Name);

            if (localisation == null || string.IsNullOrEmpty(name) /*|| string.Compare(nameToMatch, name, true) != 0*/)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.WorkplaceNotFound;
                return RedirectToAction(MVC.Home.Index());
            }
            else
            {
                var container = new SearchSingleResultViewModel { Localisation = localisation };
                return View(MVC.Localisation.Views.FullSearchResultDetail, container);
            }
        }

        [ChildActionOnly]
        public virtual ActionResult Suggestions(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var original = lRepo.Get(id);
            var suggestions = lRepo.GetMany(loc => (loc.ID != id) && (loc.TypeValue == original.TypeValue) && (loc.PostalCode == original.PostalCode));
            suggestions = MiscHelpers.Shuffle(suggestions.Where(loc => !string.IsNullOrEmpty(loc.GetMainPic()) && !loc.IsOffline).ToList());
            suggestions = suggestions.Take(5).OrderBy(x => x.Name).ToList();

            return PartialView(MVC.Localisation.Views._Suggestions, suggestions);
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

            return PartialView(MVC.Localisation.Views._MapItemSummary, localisation);
        }

        /// <summary>
        /// Action to get localisation description
        /// </summary>
        /// <param name="id">Id of the localisation</param>
        /// <returns>Redirect to returnUrl</returns>
        public virtual ActionResult MapItemLink(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);

            var localisation = lRepo.Get(id);
            if (localisation == null)
                return null;

            return Json(localisation.GetDetailFullUrl(Url), JsonRequestBehavior.AllowGet);
        }

		/// <summary>
		/// Get action resulta to show form to ask booking via form
		/// </summary>
		/// <param name="id">id of the localisation</param>
		/// <returns>booking form</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		public virtual ActionResult AskBooking(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var memberId = WebHelper.GetIdentityId(User.Identity);
			var localisation = lRepo.Get(id);
			var member = mRepo.Get(memberId);

			var container = new LocalisationAskBookingFormModel(localisation, member);
            return PartialView(MVC.Localisation.Views._AskBooking, container);
		}

		/// <summary>
		/// Get action resulta to show form to ask booking via form
		/// </summary>
		/// <param name="id">id of the localisation</param>
		/// <returns>booking form</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
        [HandleModelStateException]
		public virtual ActionResult AskBooking(int id, LocalisationAskBookingFormModel formData)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var localisation = lRepo.Get(id);

			if (ModelState.IsValid)
			{
				try
				{
					dynamic contactMail = new Email(MVC.Emails.Views.Email);
					contactMail.From = formData.Contact.FirstName + " " + formData.Contact.LastName + "<" + formData.Contact.EMail + ">";
					contactMail.To = MiscHelpers.EmailConstants.ContactMail;
					contactMail.Subject = string.Format("{0} - {1}", formData.Contact.Subject, localisation.GetDetailFullUrl(Url));
					contactMail.ToName = MiscHelpers.EmailConstants.ContactDisplayName;
					contactMail.Content = formData.Contact.Message;
					contactMail.Send();
				}
				catch (Exception ex)
				{
					_Logger.Error("AskBooking", ex);
                    ModelState.AddModelError("", ex.Message);
                    throw new ModelStateException(ModelState);
				}

				return PartialView(MVC.Shared.Views._InfoMessage, Worki.Resources.Views.Home.HomeString.MailWellSent);
			}
            throw new ModelStateException(ModelState);
		}

		/// <summary>
		/// Get action result to show contact partial view
		/// </summary>
		/// <param name="id">id of the localisation</param>
		/// <returns>contact info</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		public virtual PartialViewResult AskContact(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var localisation = lRepo.Get(id);

			return PartialView(MVC.Localisation.Views._AskContact,localisation);
		}

        /// <summary>
        /// GET action to create a new free localisation
        /// </summary>
        /// <returns>The form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        [ActionName("add-free-place")]
        public virtual ActionResult CreateFree()
        {
			return View(MVC.Localisation.Views.Edit, new LocalisationFormViewModel(true));
        }

		/// <summary>
		/// GET action to create a new bookable localisation
		/// </summary>
		/// <returns>The form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		[ActionName("add-place")]
		public virtual ActionResult CreateNotFree()
		{
            return View(MVC.Localisation.Views.Edit, new LocalisationFormViewModel(false, false));
		}

		/// <summary>
		/// GET action to create a new shared office
		/// </summary>
		/// <returns>The form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		[ActionName("add-shared-office")]
		public virtual ActionResult CreateSharedOffice()
		{
			return View(MVC.Localisation.Views.Edit, new LocalisationFormViewModel(false, true));
		}

        /// <summary>
        /// GET action to adit an existing localisation
        /// </summary>
        /// <returns>The form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        [ActionName("edit")]
        public virtual ActionResult Edit(int? id)
        {
            if (!id.HasValue)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.WorkplaceNotFound;
                return RedirectToAction(MVC.Home.Index());
            }
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var localisation = lRepo.Get(id.Value);
            if (localisation == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.WorkplaceNotFound;
                return RedirectToAction(MVC.Home.Index());
            }
			return View(new LocalisationFormViewModel(localisation));
        }

		const string LocalisationPrefix = "Localisation";

        /// <summary>
        /// POST action to edit/create a localisation
        /// upload image files or remove files -> do it via js
        /// create the localisation if id has no value, update in database if there is an id
        /// </summary>
        /// <param name="localisation">The localisation data from the form (provided from custom model binder)</param>
        /// <param name="id">The id of the edited localisation</param>
        /// <returns>the detail view of localistion if ok, the form with errors else</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
        [ActionName("edit")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(LocalisationFormViewModel localisationForm, int? id)
        {
            var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
            var field = string.Empty;
            var modifType = (!id.HasValue || id.Value == 0) ? EditionType.Creation : EditionType.Edition;
            //to keep files state in case of error
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Localisation), new PictureDataContainer(localisationForm.Localisation));

			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);

			if (modifType == EditionType.Creation)
			{
				var offerList = _ObjectStore.Get<OfferFormListModel>("OfferList");
				if (offerList != null)
				{
					localisationForm.Localisation.Offers.Clear();
					foreach (var offer in offerList.Offers)
					{
						localisationForm.Localisation.Offers.Add(offer);
					}
				}
			}
			else
			{
				var locFromDb = lRepo.Get(id.Value);
				localisationForm.Localisation.Offers.Clear();
				foreach (var offer in locFromDb.Offers)
				{
					localisationForm.Localisation.Offers.Add(offer);
				}
			}

            try
            {
                var member = mRepo.GetMember(User.Identity.Name);
                Member.Validate(member);
                if (ModelState.IsValid)
                {
                    var localisationToAdd = localisationForm.Localisation;
                    var idToRedirect = 0;
                    
                    var offerCount = 0;
                    if (modifType == EditionType.Creation)
                    {
                        localisationToAdd.SetOwner(localisationForm.IsOwner ? member.MemberId : mRepo.GetAdminId());
                        //validate
                        _SearchService.ValidateLocalisation(localisationToAdd, ref error);
                        //save
                        localisationToAdd.MemberEditions.Add(new MemberEdition { ModificationDate = DateTime.UtcNow, MemberId = member.MemberId, ModificationType = (int)EditionType.Creation });
                        lRepo.Add(localisationToAdd);
                        offerCount = localisationToAdd.Offers.Count;
                    }
                    else
                    {
                        var editionAccess = member.HasEditionAccess(Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminRole));
                        if (!string.IsNullOrEmpty(editionAccess))
                        {
                            error = editionAccess;
                            throw new Exception(editionAccess);
                        }
                        var loc = lRepo.Get(id.Value);
                        TryUpdateModel(loc, LocalisationPrefix);
                        loc.MemberEditions.Add(new MemberEdition { ModificationDate = DateTime.UtcNow, MemberId = member.MemberId, ModificationType = (int)EditionType.Edition });
                        offerCount = loc.Offers.Count;
                    }
                    if (!localisationForm.IsFreeLocalisation && offerCount == 0)
                    {
                        error = Worki.Resources.Views.Localisation.LocalisationString.MustAddOffer;
                        field = "NewOfferType";
                        throw new Exception(error);
                    }
                    context.Commit();
                    _ObjectStore.Delete(PictureData.GetKey(ProviderType.Localisation));
                    _ObjectStore.Delete("OfferList");

                    idToRedirect = modifType == EditionType.Creation ? localisationToAdd.ID : id.Value;
                    localisationForm.Localisation.ID = idToRedirect;

                    TempData[MiscHelpers.TempDataConstants.Info] = modifType == EditionType.Creation ? Worki.Resources.Views.Localisation.LocalisationString.LocHaveBeenCreate : Worki.Resources.Views.Localisation.LocalisationString.LocHaveBeenEdit;
                    if (modifType == EditionType.Creation)
                    {
                        //send welcome mail
                        if (localisationForm.SendMailToOwner && !string.IsNullOrEmpty(localisationForm.Localisation.Mail))
                        {
                            dynamic newMemberMail = new Email(MVC.Emails.Views.Email);
                            newMemberMail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
                            newMemberMail.To = localisationForm.Localisation.Mail;
                            newMemberMail.ToName = "";

                            newMemberMail.Subject = string.Format(Worki.Resources.Email.Activation.LocalisationCreate, localisationForm.Localisation.GetFullName());
                            newMemberMail.Content = string.Format(Worki.Resources.Email.Activation.LocalisationCreateContent,
                                                                    localisationForm.Localisation.GetFullName(),
                                                                    Localisation.GetOfferType(localisationForm.Localisation.TypeValue),
                                                                    localisationForm.Localisation.City,
                                                                    localisationForm.Localisation.GetDetailFullUrl(Url),
                                                                    localisationForm.Localisation.GetFullName(),
                                                                    localisationForm.Localisation.GetFullName());

                            newMemberMail.Send();
                        }
                        TempData[MiscHelpers.TempDataConstants.NewLocalisationId] = idToRedirect;
                    }
                    if (!Roles.IsUserInRole(member.Username, MiscHelpers.BackOfficeConstants.BackOfficeRole) && !localisationForm.IsFreeLocalisation && !localisationForm.IsSharedOffice)
                        return RedirectToAction(MVC.Home.Pricing());
                    else
                        return Redirect(localisationForm.Localisation.GetDetailFullUrl(Url));

                }
            }
            catch (Exception ex)
            {
                _Logger.Error("Edit", ex);
                context.Complete();
                ModelState.AddModelError(field, error);
            }
			localisationForm.Localisation.ID = id ?? 0;
			return View(new LocalisationFormViewModel(localisationForm.Localisation));
        }

        const string returnUrlPostComment = "returnUrlPostComment";

        /// <summary>
        /// POST Action result to post a comment on a localisation
        /// </summary>
        /// <param name="id">The id of the comment's localisation</param>
        /// <param name="com">The comment data from the form</param>
        /// <returns>redirect to the return urlif ok, show errors else</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		//[ValidateAntiForgeryToken]
		[HandleModelStateException]
		public virtual PartialViewResult PostComment(int id, Comment com)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
			var localisation = lRepo.Get(id);
			try
			{
				var member = mRepo.GetMember(User.Identity.Name);
				Member.Validate(member);

				if (ModelState.IsValid)
				{
					com.Localisation = localisation;
                    com.PostUserID = member.MemberId;
                    com.Date = System.DateTime.UtcNow;
					com.Validate(ref  error);

                    localisation.Comments.Add(com);

					context.Commit();
					return PartialView(MVC.Shared.Views._LocalisationSingleComment, com);
				}
				else
				{
					throw new ModelStateException(ModelState);
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("PostComment", ex);
				context.Complete();
				ModelState.AddModelError("", error);
				throw new ModelStateException(ModelState);
			}
		}

        /// <summary>
        /// GET Action result to delete a comment, only available for admin role
        /// redirect to localisation admin page
        /// </summary>
        /// <param name="id">The id of the comment's localisation</param>
        /// <param name="commentId">The id of the comment</param>
        /// <param name="returnUrl">The comment data from the form</param>
        /// <returns>redirect to the return url if ok, show errors else</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult DeleteComment(int id, int commentId, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			try
			{
				var localisation = lRepo.Get(id);
				foreach (var comment in localisation.Comments.ToList())
				{
					if (comment.ID == commentId)
					{
						localisation.Comments.Remove(comment);
					}
				}
				context.Commit();
			}
			catch (Exception ex)
			{
				context.Complete();
				_Logger.Error(ex.Message);
			}
			return Redirect(returnUrl);
		}

		[AcceptVerbs(HttpVerbs.Get), Authorize]
        [ActionName("set-ownership")]
		public virtual ActionResult SetOwnership(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			var localisation = lRepo.Get(id);
			try
			{
				var memberId = WebHelper.GetIdentityId(User.Identity);
                localisation.SetOwner(memberId);
				var member = mRepo.Get(memberId);
				if (member == null || member.MemberMainData == null)
				{
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Profile.ProfileString.MemberNotFound;
					return RedirectToAction(MVC.Home.Index());
				}
				var dest = member.Email;
				//send mail to member
				dynamic Ownermail = new Email(MVC.Emails.Views.Email);
				Ownermail.From = MiscHelpers.EmailConstants.ContactDisplayName + "<" + MiscHelpers.EmailConstants.ContactMail + ">";
				Ownermail.To = dest;
				Ownermail.ToName = member.MemberMainData.FirstName;
				Ownermail.Subject = string.Format(Worki.Resources.Email.Common.OwnershipSubject, localisation.Name);
				Ownermail.Content = string.Format(Worki.Resources.Email.Common.Ownership, localisation.Name,
										Url.ActionAbsolute(MVC.Localisation.Edit(id)));

                context.Commit();

                Ownermail.Send();

                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.YouAreOwner;
			}
			catch (Exception ex)
			{
				context.Complete();
				_Logger.Error("SetOwnership", ex);
			}

            return RedirectToAction(MVC.Home.Pricing());
		}

        /// <summary>
        /// action to ask for gb publish
        /// </summary>
        /// <param name="id">The id of the localisation</param>
        /// <returns>view containing ask</returns>
        public virtual ActionResult OpenGraphInvitation(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var localisation = lRepo.Get(id);

            return PartialView(MVC.Localisation.Views._AddPlaceFacebook, localisation);
        }

		/// <summary>
		/// POST action to publish facebook opengraph action
		/// </summary>
		/// <param name="id">The id of the localisation</param>
		/// <param name="accessToken">fb access token</param>
        /// <param name="type">type of action</param>
		/// <returns>success if ok</returns>
        [AcceptVerbs(HttpVerbs.Post)]
		public virtual ActionResult AddToOpenGraph(int id, string accessToken, string type)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var localisation = lRepo.Get(id);

			var action = string.Format("/me/{0}:{1}", WebHelper.GetFacebookNamespace(), type);

			try
			{
				FacebookClient fbClient = new FacebookClient(accessToken);
                fbClient.Post(action, new Dictionary<string, object> { { "workspace", localisation.GetDetailFullUrl(Url) } });
                return PartialView(MVC.Shared.Views._InfoMessage, Worki.Resources.Views.Localisation.LocalisationString.FacebookUpdated);
			}
			catch (Exception ex)
			{
                _Logger.Error("AddToOpenGraph", ex);
				return Json(ex.Message);
			}
		}

        /// <summary>
        /// Action to add a localisation to member favorites
        /// </summary>
        /// <param name="locId">Id of the favorite localisation</param>
        /// <param name="returnUrl">Url to redirect when action done</param>
        /// <returns>Redirect to returnUrl</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
        public virtual ActionResult AddFavorite(int id)
        {
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var loc = lRepo.Get(id);
            try
            {
                var memberId = WebHelper.GetIdentityId(User.Identity);
                var member = mRepo.Get(memberId);
                if (member == null)
                    return null;
                if (member.FavoriteLocalisations.Count(fl => fl.LocalisationId == id) == 0)
                {
                    member.FavoriteLocalisations.Add(new FavoriteLocalisation { LocalisationId = id });
                    context.Commit();
                }

                return Json("added");
            }

            catch (Exception ex)
            {
                context.Complete();
                _Logger.Error("AddFavorite", ex);
                return Json(ex.Message);
            }
        }

		#region Search

        /// <summary>
        /// Action to get partial view of search form
        /// </summary>
        /// <param name="searchType">enum within eSearchType</param>
        /// <returns>corresponding partial form</returns>
        public virtual PartialViewResult SearchForm(string searchType, string place)
        {
            var searchEnum = (eSearchType)Enum.Parse(typeof(eSearchType), searchType);

            SearchCriteriaFormViewModel model = null;
            switch (searchEnum)
            {
                case eSearchType.ePerOffer:
                    {
                        var criteria = new SearchCriteria(true);
                        criteria.Place = place;
                        model = new SearchCriteriaFormViewModel(criteria);
                    }
                    break;
                case eSearchType.ePerType:
                    {
                        var criteria = new SearchCriteria { SearchType = eSearchType.ePerType };
                        criteria.Place = place;
                        model = new SearchCriteriaFormViewModel(criteria);
                    }
                    break;
                case eSearchType.ePerName:
                    {
                        var criteria = new SearchCriteria(true);
                        criteria.Place = place;
                        criteria.SearchType = eSearchType.ePerName;
                        criteria.OrderBy = eOrderBy.Rating;
                        model = new SearchCriteriaFormViewModel(criteria);
                    }
                    break;
                default:
                    break;
            }
            return PartialView(MVC.Localisation.Views._SearchForm, model);
        }

		/// <summary>
		/// GET Action result to search localisations from a SearchCriteria
		/// the search is per offer (free area, meeting room etc...)
		/// </summary>
		/// <returns>the form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		[ActionName("search")]
		public virtual ActionResult FullSearch()
		{
			var criteria = new SearchCriteria(true);
            return View(MVC.Home.Views.Index, new SearchCriteriaFormViewModel(criteria));
		}

		[AcceptVerbs(HttpVerbs.Get)]
        [ActionName("search-per-type")]
		public virtual ActionResult FullSearchPerType()
		{
            var criteria = new SearchCriteria { SearchType = eSearchType.ePerType };
            return View(MVC.Home.Views.Index, new SearchCriteriaFormViewModel(criteria));
		}


        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("search-per-name")]
        public virtual ActionResult FullSearchPerName()
        {
            var criteria = new SearchCriteria(true, eSearchType.ePerName, eOrderBy.Rating);
            return View(MVC.Home.Views.Index, new SearchCriteriaFormViewModel(criteria));
        }

        /// <summary>
        /// action which redirect to search results, for seo purpose
        /// </summary>
        /// <param name="localisationType">localisation type</param>
        /// <param name="localisationPlace">localisation place</param>
        /// <returns>redirect to corresponding results</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult FullSearchByTypeSeo(string type, string place)
        {
            var criteria = new SearchCriteria();
			criteria.Place = place;
            switch (type)
            {
                case MiscHelpers.SeoConstants.CoworkingSpace:
                    criteria.CoworkingSpace = true;
                    break;
				case MiscHelpers.SeoConstants.Telecentre:
                    criteria.Telecentre = true;
                    break;
				case MiscHelpers.SeoConstants.SharedOffice:
					criteria.SharedOffice = true;
                    break;
				case MiscHelpers.SeoConstants.BuisnessCenter:
                    criteria.BuisnessCenter = true;
                    break;
				case MiscHelpers.SeoConstants.FreeAreas:
					criteria.FreeAreas = true;
                    break;
                case MiscHelpers.SeoConstants.OtherPlaces:
					criteria.OtherTypes = true;
                    break;
            }
            criteria.Everything = false;

			var criteriaViewModel = _SearchService.FillSearchResults(criteria);

			criteriaViewModel.FillPageInfo();
			return View(MVC.Localisation.Views.FullSearchResult, criteriaViewModel);
        }

        /// <summary>
        /// action which redirect to search results, for seo purpose
        /// </summary>
        /// <param name="offerType">offer type</param>
        /// <param name="localisationPlace">localisation place</param>
        /// <returns>redirect to corresponding results</returns>
        [AcceptVerbs(HttpVerbs.Get)]
		public virtual ActionResult FullSearchByOfferSeo(string offerType, string place)
        {
            var criteria = new SearchCriteria();
			criteria.Place = place;
            criteria.OfferData.Type = Localisation.GetOfferTypeFromSeoString(offerType);

			var criteriaViewModel = _SearchService.FillSearchResults(criteria);

			criteriaViewModel.FillPageInfo();
			return View(MVC.Localisation.Views.FullSearchResult, criteriaViewModel);
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
            return View(MVC.Home.Views.Index, new SearchCriteriaFormViewModel(criteria));
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

			criteriaViewModel.FillPageInfo(pageValue);
			return View(MVC.Localisation.Views.FullSearchResult, criteriaViewModel);
		}

        #region Ajax

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
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var localisations = lRepo.FindSimilarLocalisation(latitude, longitude);
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
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var localisations = lRepo.GetMany(item => (item.MainLocalisation.IsMain && !item.MainLocalisation.IsOffline && item.LocalisationFiles.Where(f => f.IsDefault == true).Count() != 0));
            var jsonLocalisations = localisations.Select(item =>
            {
                return item.GetJson(this);
            });
            return Json(jsonLocalisations.ToList());
        }

		JsonResult GetSearchResult(SearchCriteriaFormViewModel criteriaViewModel)
		{
            var orderResult = this.RenderRazorViewToString(MVC.Localisation.Views._SearchOrderSelector, criteriaViewModel);
            var titleResult = string.Format(Worki.Resources.Views.Search.SearchString.YourSearchResult, criteriaViewModel.List.Count);
            switch (criteriaViewModel.Criteria.ResultView)
            {
                case eResultView.Map:
                    {
                        var locList = (from item in criteriaViewModel.Criteria.Projection select item.GetJson());
                        return Json(new { order = orderResult, title = titleResult, localisations = locList, place = criteriaViewModel.Criteria.Place }, JsonRequestBehavior.AllowGet);
                    }
                case eResultView.List:
                default:
                    {
                        var listResult = this.RenderRazorViewToString(MVC.Localisation.Views._SearchResults, criteriaViewModel);
                        var locList = (from item in criteriaViewModel.PageResults select item.GetJson());
                        return Json(new { list = listResult, order = orderResult, title = titleResult, localisations = locList, place = criteriaViewModel.Criteria.Place }, JsonRequestBehavior.AllowGet);
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
        public virtual ActionResult AjaxFullSearch(SearchCriteria criteria)
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
                    _Logger.Error("AjaxFullSearch", ex);
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
        public virtual ActionResult AjaxFullSearchResult(int? page)
        {
            var pageValue = page ?? 1;
            var criteria = _SearchService.GetCriteria(Request, pageValue);
            var criteriaViewModel = _SearchService.FillSearchResults(criteria);

            criteriaViewModel.FillPageInfo(pageValue);
			return GetSearchResult(criteriaViewModel);
        }


        #endregion

		#endregion
	}
}
