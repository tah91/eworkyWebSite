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

namespace Worki.Web.Controllers
{
	public partial class LocalisationController : ControllerBase
    {
        ILogger _Logger;
		ISearchService _SearchService;

		public LocalisationController(ILogger logger, ISearchService searchService)
		{
			_Logger = logger;
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
            var nameToMatch = MiscHelpers.GetSeoString(localisation.Name);

			if (localisation == null || string.IsNullOrEmpty(name) /*|| string.Compare(nameToMatch, name, true) != 0*/)
            {
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Localisation.LocalisationString.WorkplaceNotFound;
                return RedirectToAction(MVC.Home.Index());
            }
			else
			{
				var container = new SearchSingleResultViewModel { Localisation = localisation };
				return View(MVC.Localisation.Views.resultats_detail, container);
			}
		}

		/// <summary>
		/// The view containing the offers of a localisation
		/// </summary>
		/// <param name="id">id of the localisation</param>
		/// <param name="type">offer type</param>
		/// <returns>The action result.</returns>
		[ActionName("offres")]
		public virtual ActionResult Offers(int id, int type)
		{
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var localisation = lRepo.Get(id);

			var container = new LocalisationOfferViewModel(localisation, (LocalisationOffer)type);
			return View(MVC.Localisation.Views.Offers, container);
		}

        /// <summary>
        /// GET action to create a new free localisation
        /// </summary>
        /// <returns>The form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        [ActionName("ajouter-lieu-gratuit")]
        public virtual ActionResult CreateFree()
        {
			return View(MVC.Localisation.Views.editer, new LocalisationFormViewModel(true));
        }

		/// <summary>
		/// GET action to create a new bookable localisation
		/// </summary>
		/// <returns>The form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		[ActionName("ajouter-lieu-payant")]
		public virtual ActionResult CreateNotFree()
		{
			return View(MVC.Localisation.Views.editer, new LocalisationFormViewModel(false));
		}

		/// <summary>
		/// GET action to create a new shared office
		/// </summary>
		/// <returns>The form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		[ActionName("ajouter-espace-partagé")]
		public virtual ActionResult CreateSharedOffice()
		{
			return View(MVC.Localisation.Views.editer, new LocalisationFormViewModel(false, true));
		}

        /// <summary>
        /// GET action to adit an existing localisation
        /// </summary>
        /// <returns>The form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        [ActionName("editer")]
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
		[ActionName("editer")]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Edit(LocalisationFormViewModel localisationForm, int? id, string addOffer)
		{
			var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
            var field = string.Empty;
			//to keep files state in case of error
			TempData[PictureData.PictureDataString] = new PictureDataContainer(localisationForm.Localisation);
			var context = ModelFactory.GetUnitOfWork();
			var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			try
			{
				var member = mRepo.GetMember(User.Identity.Name);
				Member.Validate(member);
				if (ModelState.IsValid)
				{
					var localisationToAdd = new Localisation();
					var idToRedirect = 0;
					var modifType = (!id.HasValue || id.Value == 0) ? EditionType.Creation : EditionType.Edition;
                    var offerCount = 0;
					if (modifType == EditionType.Creation)
					{
						//update
						UpdateModel(localisationToAdd, LocalisationPrefix);
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
						UpdateModel(loc, LocalisationPrefix);
                        loc.SetOwner(localisationForm.IsOwner ? member.MemberId : -1);
						loc.MemberEditions.Add(new MemberEdition { ModificationDate = DateTime.UtcNow, MemberId = member.MemberId, ModificationType = (int)EditionType.Edition });
                        offerCount = loc.Offers.Count;
					}
                    if (string.IsNullOrEmpty(addOffer) && !localisationForm.IsFreeLocalisation && offerCount == 0)
                    {
                        error = Worki.Resources.Views.Localisation.LocalisationString.MustAddOffer;
                        field = "NewOfferType";
                        throw new Exception(error);
                    }
					context.Commit();
					TempData.Remove(PictureData.PictureDataString);

					idToRedirect = modifType == EditionType.Creation ? localisationToAdd.ID : id.Value;
					localisationForm.Localisation.ID = idToRedirect;
					if (!string.IsNullOrEmpty(addOffer))
					{
						return RedirectToAction(MVC.Offer.Create(idToRedirect, localisationForm.NewOfferType));
					}
					else
					{
						TempData[MiscHelpers.TempDataConstants.Info] = modifType == EditionType.Creation ? Worki.Resources.Views.Localisation.LocalisationString.LocHaveBeenCreate : Worki.Resources.Views.Localisation.LocalisationString.LocHaveBeenEdit;
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
        [ActionName("devenir-proprietaire")]
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

			return Redirect(localisation.GetDetailFullUrl(Url));
		}

		#region Search

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

        /// <summary>
        /// Action to get partial view of search form
        /// </summary>
        /// <param name="searchType">enum within eSearchType</param>
        /// <param name="directAccessType">enum within eDirectAccessType</param>
        /// <returns>corresponding partial form</returns>
        public virtual PartialViewResult SearchForm(string searchType, string directAccessType, string place)
        {
            var searchEnum = (eSearchType)Enum.Parse(typeof(eSearchType), searchType);
            var directAccessEnum = (eDirectAccessType)Enum.Parse(typeof(eDirectAccessType), directAccessType);

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
                        var criteria = SearchCriteria.CreateSearchCriteria(directAccessEnum);
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
		[ActionName("recherche")]
		public virtual ActionResult FullSearch()
		{
			var criteria = new SearchCriteria(true);
            return View(MVC.Home.Views.index, new SearchCriteriaFormViewModel(criteria));
		}

		[AcceptVerbs(HttpVerbs.Get)]
		[ActionName("recherche-par-type")]
		public virtual ActionResult FullSearchPerType()
		{
            var criteria = SearchCriteria.CreateSearchCriteria(eDirectAccessType.eNone);
            return View(MVC.Home.Views.index, new SearchCriteriaFormViewModel(criteria));
		}


        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("recherche-par-nom")]
        public virtual ActionResult FullSearchPerName()
        {
            var criteria = new SearchCriteria(true, eSearchType.ePerName, eOrderBy.Rating);
            return View(MVC.Home.Views.index, new SearchCriteriaFormViewModel(criteria));
        }

		/// <summary>
		/// GET Action result to search localisations from a SearchCriteria
		/// the search is per localisation type (wifi spot, hotel, resto etc...)
		/// the SearchCriteria is pre filled according to the profile (students, teleworker etc...)
		/// </summary>
		/// <returns>the form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get)]
		[ActionName("recherche-special")]
		public virtual ActionResult FullSearchPerTypeSpecial(int type)
		{
            var criteria = SearchCriteria.CreateSearchCriteria((eDirectAccessType)type);
			return View(MVC.Localisation.Views.recherche, new SearchCriteriaFormViewModel(criteria));
		}

        /// <summary>
        /// action which redirect to search results, for seo purpose
        /// </summary>
        /// <param name="localisationType">localisation type</param>
        /// <param name="localisationPlace">localisation place</param>
        /// <returns>redirect to corresponding results</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult FullSearchByTypeSeo(string type, string lieu)
        {
            var criteria = new SearchCriteria();
            criteria.Place = lieu;
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
			return View(MVC.Localisation.Views.resultats_liste, criteriaViewModel);
        }

        /// <summary>
        /// action which redirect to search results, for seo purpose
        /// </summary>
        /// <param name="offerType">offer type</param>
        /// <param name="localisationPlace">localisation place</param>
        /// <returns>redirect to corresponding results</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult FullSearchByOfferSeo(string offerType, string lieu)
        {
            var criteria = new SearchCriteria();
            criteria.Place = lieu;
            criteria.OfferData.Type = Localisation.GetOfferTypeFromSeoString(offerType);

			var criteriaViewModel = _SearchService.FillSearchResults(criteria);

			criteriaViewModel.FillPageInfo();
			return View(MVC.Localisation.Views.resultats_liste, criteriaViewModel);
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
		[ActionName("recherche")]
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
            return View(MVC.Home.Views.index, new SearchCriteriaFormViewModel(criteria));
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
			return View(MVC.Localisation.Views.resultats_detail, detailModel);
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

		#endregion
	}
}
