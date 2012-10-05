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
using Worki.Infrastructure.Email;
using System.Collections.Generic;
using Facebook;
using System.Net.Mail;

namespace Worki.Web.Controllers
{
	public partial class LocalisationController : ControllerBase
    {
		ISearchService _SearchService;

        public LocalisationController(ILogger logger, IObjectStore objectStore, IEmailService emailService, ISearchService searchService)
            : base(logger, objectStore, emailService)
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
        [ActionName("details")]
        public virtual ActionResult Details(int id, string name)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var localisation = lRepo.Get(id);

            if (localisation == null || string.IsNullOrEmpty(name) /*|| string.Compare(MiscHelpers.GetSeoString(localisation.Name), name, true) != 0*/)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.WorkplaceNotFound;
                return RedirectToAction(MVC.Home.Index());
            }
            else
            {
                var container = new SearchSingleResultViewModel { Localisation = localisation };
                if (localisation.IsOffline)
                {
                    ModelState.AddModelError("", Worki.Resources.Views.Localisation.LocalisationString.PlaceNotAvailable);
                }
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
                    var contactMailContent = Worki.Resources.Email.Activation.BOAskContent;

                    var contactMail = _EmailService.PrepareMessageToDefault(new MailAddress(formData.Contact.EMail, formData.Contact.FirstName + " " + formData.Contact.LastName),
                        string.Format("{0} - {1}", formData.Contact.Subject, localisation.GetDetailFullUrl(Url)),
                        WebHelper.RenderEmailToString(formData.Contact.FirstName + " " + formData.Contact.LastName, formData.Contact.Message));

                    _EmailService.Deliver(contactMail);
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

            return PartialView(MVC.Localisation.Views._AskContact, localisation);
        }

        public virtual ActionResult PutOfflineAndRedirect(int id, string redirect)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var localisation = lRepo.Get(id);
            var redirectUrl = string.IsNullOrEmpty(redirect) ? localisation.GetDetailFullUrl(Url) : redirect;
            try
            {
                localisation.MainLocalisation.IsOffline = true;
                context.Commit();
                TempData[MiscHelpers.TempDataConstants.Info] = "Le lieu est bien hors-ligne.";
            }
            catch (Exception ex)
            {
                context.Complete();
                _Logger.Error("PutOfflineAndRedirect", ex);
            }

            return Redirect(redirectUrl);
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


        void CompleteEdition(EditionType editionType, Localisation localisation)
        {
            TempData[MiscHelpers.TempDataConstants.Info] = editionType == EditionType.Creation ?
                                                                    Worki.Resources.Views.Localisation.LocalisationString.LocHaveBeenCreate :
                                                                    Worki.Resources.Views.Localisation.LocalisationString.LocHaveBeenEdit;

            if (editionType == EditionType.Creation)
            {
                //send welcome mail
                if (!string.IsNullOrEmpty(localisation.Mail))
                {
                    var newMemberMailContent =string.Format(Worki.Resources.Email.Activation.LocalisationCreateContent,
                                                            localisation.GetFullName(),
                                                            Localisation.GetOfferType(localisation.TypeValue),
                                                            localisation.City,
                                                            localisation.GetDetailFullUrl(Url),
                                                            localisation.GetFullName(),
                                                            localisation.GetFullName());

                    var newMemberMail = _EmailService.PrepareMessageFromDefault(new MailAddress(localisation.Mail),
                        string.Format(Worki.Resources.Email.Activation.LocalisationCreate, localisation.GetFullName()),
                        WebHelper.RenderEmailToString("", newMemberMailContent));

                    _EmailService.Deliver(newMemberMail);
                }

                //need for pricing page
                TempData[MiscHelpers.TempDataConstants.NewLocalisationId] = localisation.ID;
            }
        }

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

            try
            {
                var member = mRepo.GetMember(User.Identity.Name);
                Member.Validate(member);
                if (ModelState.IsValid)
                {
                    var localisationToAdd = localisationForm.Localisation;

                    if (modifType == EditionType.Creation)
                    {
                        //add owner
                        localisationToAdd.SetOwner(localisationForm.IsOwner ? member.MemberId : mRepo.GetAdminId());
                        //validate
                        _SearchService.ValidateLocalisation(localisationToAdd, ref error);
                        //add to repo
                        localisationToAdd.MemberEditions.Add(new MemberEdition { ModificationDate = DateTime.UtcNow, MemberId = member.MemberId, ModificationType = (int)EditionType.Creation });
                        //if paying, offline till no offer added
                        if (!localisationForm.IsFreeLocalisation)
                            localisationToAdd.MainLocalisation.IsOffline = true;
                        lRepo.Add(localisationToAdd);
                    }
                    else
                    {
                        //validate
                        var editionAccess = member.HasEditionAccess(Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminRole));
                        if (!string.IsNullOrEmpty(editionAccess))
                        {
                            error = editionAccess;
                            throw new Exception(editionAccess);
                        }
                        var loc = lRepo.Get(id.Value);
                        //update repo
                        TryUpdateModel(loc, LocalisationPrefix);
                        loc.MemberEditions.Add(new MemberEdition { ModificationDate = DateTime.UtcNow, MemberId = member.MemberId, ModificationType = (int)EditionType.Edition });
                    }

                    context.Commit();
                    _ObjectStore.Delete(PictureData.GetKey(ProviderType.Localisation));

                    //set localisation Id
                    localisationForm.Localisation.ID = modifType == EditionType.Creation ? 
                                                        localisationToAdd.ID :
                                                        id.Value;

                    //case paying loc => go to offer edition
                    if (!localisationForm.IsFreeLocalisation)
                    {
                        return RedirectToAction(MVC.Localisation.EditOffers(localisationForm.Localisation.ID, modifType));
                    }
                    //case paying free loc => go to the edited / created page
                    else
                    {
                        CompleteEdition(modifType, localisationForm.Localisation);
                        return Redirect(localisationForm.Localisation.GetDetailFullUrl(Url));
                    }

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

        /// <summary>
        /// Return view to edit localisation offers
        /// </summary>
        /// <param name="id">loc id</param>
        /// <returns>view containing offers to edit/add</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        public virtual ActionResult EditOffers(int id, EditionType editionType)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var localisation = lRepo.Get(id);
            if (localisation == null)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.WorkplaceNotFound;
                return RedirectToAction(MVC.Home.Index());
            }
            return View(new OfferCounterModel(localisation) { EditionType = editionType });
        }

        /// <summary>
        /// Return view to edit localisation offers
        /// </summary>
        /// <param name="id">loc id</param>
        /// <returns>view containing offers to edit/add</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
        [HandleModelStateException]
        public virtual ActionResult EditOffers(int id, bool isEnd, OfferCounterModel formData)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    int currentNeed;
                    string helpText;
                    LocalisationOffer offerType;
                    var context = ModelFactory.GetUnitOfWork();
                    var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
                    var localisation = lRepo.Get(id);
                    formData.RefreshData(localisation);

                    if (formData.NeedAddOffer(out offerType, out currentNeed, out helpText))
                    {
                        var newForm = this.RenderRazorViewToString(MVC.Offer.Views._AjaxAdd, new OfferFormViewModel(formData.IsSharedOffice, offerType, currentNeed) { LocId = id });
                        return Json(new { help = helpText, form = newForm });
                    }
                    else
                    {
                        if (isEnd)
                        {
                            if (formData.OfferLists.Count == 0)
                            {
                                throw new Exception( Worki.Resources.Views.Localisation.LocalisationString.MustAddOffer);
                            }
                        }
                        return Json(new { help = "", form = "", completed = "true", editionType = formData.EditionType });
                    }
                }
                catch (Exception ex)
                {
                    _Logger.Error("EditOffers", ex);
                    ModelState.AddModelError("", ex.Message);
                    throw new ModelStateException(ModelState);
                }
            }
            throw new ModelStateException(ModelState);
        }

        [AcceptVerbs(HttpVerbs.Get), Authorize]
        public virtual ActionResult EditOffersEnd(int id, EditionType editionType)
        {
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var localisation = lRepo.Get(id);
            var member = mRepo.GetMember(User.Identity.Name);
            CompleteEdition(editionType, localisation);

            //if creation, put it online as everything is fine
            if (editionType == EditionType.Creation)
            {
                try
                {
                    localisation.MainLocalisation.IsOffline = false;
                    context.Commit();
                }
                catch (Exception ex)
                {
                    _Logger.Error("EditOffersEnd", ex);
                    context.Complete();
                    return RedirectToAction(MVC.Home.Error());
                }
            }
            var newContext = ModelFactory.GetUnitOfWork();
            lRepo = ModelFactory.GetRepository<ILocalisationRepository>(newContext);
            localisation = lRepo.Get(id);
            if (!Roles.IsUserInRole(member.Username, MiscHelpers.BackOfficeConstants.BackOfficeRole) && !localisation.IsSharedOffice())
            {
                return RedirectToAction(MVC.Home.Pricing());
            }
            else
            {
                return Redirect(localisation.GetDetailFullUrl(Url));
            }
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
                var ownermailContent =string.Format(Worki.Resources.Email.Common.Ownership, localisation.Name,
										Url.ActionAbsolute(MVC.Localisation.Edit(id)));

                var ownermail = _EmailService.PrepareMessageFromDefault(new MailAddress(member.Email, member.MemberMainData.FirstName),
                    string.Format(Worki.Resources.Email.Common.OwnershipSubject, localisation.Name),
                    WebHelper.RenderEmailToString(member.MemberMainData.FirstName, ownermailContent));

                context.Commit();

                _EmailService.Deliver(ownermail);

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
        [HandleModelStateException]
        public virtual ActionResult FullSearch(SearchCriteria criteria)
        {
            var isAjax = Request.IsAjaxRequest();
            if (ModelState.IsValid)
            {
                try
                {
                    if (isAjax)
                    {
                        var criteriaViewModel = _SearchService.FillSearchResults(criteria);
                        return GetSearchResult(criteriaViewModel);
                    }
                    else
                    {
                        var rvd = _SearchService.GetRVD(criteria);
                        return RedirectToAction(MVC.Localisation.Actions.ActionNames.FullSearchResult, rvd);
                    }
                }
                catch (Exception ex)
                {
                    _Logger.Error("FullSearch", ex);
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.CheckCriterias);
                    if (isAjax)
                    {
                        throw new ModelStateException(ModelState);
                    }
                }
            }
            if (isAjax)
            {
                throw new ModelStateException(ModelState);
            }
            else
            {
                return View(MVC.Home.Views.Index, new SearchCriteriaFormViewModel(criteria));
            }
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

            if (Request.IsAjaxRequest())
            {
                return GetSearchResult(criteriaViewModel);
            }
            else
            {
                return View(MVC.Localisation.Views.FullSearchResult, criteriaViewModel);
            }            
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
            var panelForm = this.RenderRazorViewToString(MVC.Localisation.Views._SearchPanelForm, criteriaViewModel);
            var rvd = _SearchService.GetRVD(criteriaViewModel.Criteria);
            var url = Url.Action(MVC.Localisation.Actions.ActionNames.FullSearchResult, rvd);

            switch (criteriaViewModel.Criteria.ResultView)
            {
                case eResultView.Map:
                    {
                        var locList = (from item in criteriaViewModel.Criteria.Projection select item.GetJson());
                        return Json(new { order = orderResult, title = titleResult, localisations = locList, place = criteriaViewModel.Criteria.Place, url = url, form = panelForm }, JsonRequestBehavior.AllowGet);
                    }
                case eResultView.List:
                default:
                    {
                        var listResult = this.RenderRazorViewToString(MVC.Localisation.Views._SearchResults, criteriaViewModel);
                        var locList = (from item in criteriaViewModel.PageResults select item.GetJson());
                        return Json(new { list = listResult, order = orderResult, title = titleResult, localisations = locList, place = criteriaViewModel.Criteria.Place, url = url, form = panelForm }, JsonRequestBehavior.AllowGet);
                    }
            }

		}

        #endregion

		#endregion
	}
}
