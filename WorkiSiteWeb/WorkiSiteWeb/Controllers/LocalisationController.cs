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
using System.Net;
using Newtonsoft.Json.Linq;

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

        public static string RequestToken = "https://foursquare.com/oauth2/authenticate?client_id={0}&response_type=code&redirect_uri={1}";
        public static string RequestTokenAccept = "https://foursquare.com/oauth2/access_token?client_id={0}&client_secret={1}&grant_type=authorization_code&redirect_uri={2}&code={3}";

        public static string ClientId = "DFOFTOSU2VYAO5TKCENWR1PIIRBHY1B4HGEZVSUAI3EIURFA";
        public static string ClientSecret = "HK4W330SMCPM3HRBLHZGCKPQC2GTJRA2XCNXSN3LCCF4Z5JM";
        public static string RedirectUrl = "http://localhost:4119/caf%C3%A9-restaurant/9/the-frog-and-rosbif";
        
        public static string FoursquareSearch = "https://api.foursquare.com/v2/venues/search?oauth_token={0}";
        public static string FoursquareTips = "https://api.foursquare.com/v2/venues/{0}/tips?oauth_token={1}";

        string GetTokenKey()
        {
            if (Session["4SQTokenKey"] != null)
                return (string)Session["4SQTokenKey"];
            else return string.Empty;
        }

        void SetTokenKey(string key)
        {
            Session["4SQTokenKey"] = key;
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

                var redirect = string.Format(RequestToken, ClientId, RedirectUrl);
                Redirect(redirect);

                var accept = String.Format(RequestTokenAccept, ClientId, ClientSecret, RedirectUrl, "CODE");
                using (var client0 = new WebClient())
                {
                    client0.Encoding = System.Text.Encoding.UTF8;
                    try
                    {
                        string textString0 = client0.DownloadString(accept);
                        JObject venuesJson0 = JObject.Parse(textString0);
                        SetTokenKey((string)venuesJson0["access_token"]);
                    }
                    catch (WebException)
                    {
                    }
                }

                using (var client = new WebClient())
                {
                    client.Encoding = System.Text.Encoding.UTF8;
                    try
                    {
                        var path = string.Format(FoursquareSearch, GetTokenKey());
                        var query = path + "&ll=" + localisation.Latitude + "," + localisation.Longitude + "&intent=match";
                        string textString = client.DownloadString(query);
                        JObject venuesJson = JObject.Parse(textString);
                        var venues = venuesJson["response"]["venues"];
                        var venue = venues[0];
                        path = string.Format(FoursquareTips, (string)venue["id"], GetTokenKey());
                        textString = client.DownloadString(path);
                        JObject tipsJSON = JObject.Parse(textString);
                        var tips = tipsJSON["response"]["items"];
                        foreach (var tip in tips)
                        {
                            var com = new Comment { Date = new DateTime(long.Parse((string)tip["createdAt"])), Post = (string)tip["text"], Member = new Member { MemberMainData = new MemberMainData { FirstName = (string)tip["user"]["firstName"], LastName = (string)tip["user"]["lastName"], Avatar = (string)tip["user"]["photo"] } } };
                            container.Localisation.Comments.Add(com);
                        }
                    }
                    catch (WebException)
                    {
                    }
                }

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
				ModelState.AddModelError("", error);
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
                        model = new SearchCriteriaFormViewModel(criteria, false);
                    }
                    break;
                case eSearchType.ePerType:
                    {
                        var criteria = SearchCriteria.CreateSearchCriteria(directAccessEnum);
                        criteria.Place = place;
                        model = new SearchCriteriaFormViewModel(criteria, false);
                    }
                    break;
                case eSearchType.ePerName:
                    {
                        var criteria = new SearchCriteria(true);
                        criteria.Place = place;
                        criteria.SearchType = eSearchType.ePerName;
                        criteria.OrderBy = eOrderBy.Rating;
                        model = new SearchCriteriaFormViewModel(criteria, false);
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
			return View(new SearchCriteriaFormViewModel(criteria, false));
		}

		[AcceptVerbs(HttpVerbs.Get)]
		[ActionName("recherche-par-type")]
		public virtual ActionResult FullSearchPerType()
		{
            var criteria = SearchCriteria.CreateSearchCriteria(eDirectAccessType.eNone);
			return View(MVC.Localisation.Views.recherche, new SearchCriteriaFormViewModel(criteria, false));
		}


        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("recherche-par-nom")]
        public virtual ActionResult FullSearchPerName()
        {
            var criteria = new SearchCriteria(true, eSearchType.ePerName, eOrderBy.Rating);
            return View(MVC.Localisation.Views.recherche, new SearchCriteriaFormViewModel(criteria, false));
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
			return View(MVC.Localisation.Views.recherche, new SearchCriteriaFormViewModel(criteria, false));
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
                case MiscHelpers.SeoConstants.SpotWifi:
                    criteria.SpotWifi = true;
                    break;
                case MiscHelpers.SeoConstants.CoffeeResto:
                    criteria.CoffeeResto = true;
                    break;
                case MiscHelpers.SeoConstants.Biblio:
                    criteria.Biblio = true;
                    break;
                case MiscHelpers.SeoConstants.PublicSpace:
                    criteria.PublicSpace = true;
                    break;
                case MiscHelpers.SeoConstants.TravelerSpace:
                    criteria.TravelerSpace = true;
                    break;
                case MiscHelpers.SeoConstants.Telecentre:
                    criteria.Telecentre = true;
                    break;
                case MiscHelpers.SeoConstants.BuisnessCenter:
                    criteria.BuisnessCenter = true;
                    break;
                case MiscHelpers.SeoConstants.CoworkingSpace:
                    criteria.CoworkingSpace = true;
                    break;
                case MiscHelpers.SeoConstants.WorkingHotel:
                    criteria.WorkingHotel = true;
                    break;
                case MiscHelpers.SeoConstants.PrivateArea:
                    criteria.PrivateArea = true;
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
		[ValidateOnlyIncomingValues]
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
