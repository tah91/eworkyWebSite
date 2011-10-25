using System;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure;
using Worki.Infrastructure.Logging;
using Worki.Service;
using Worki.Infrastructure.Repository;
using Worki.Infrastructure.Helpers;
using Postal;

namespace Worki.Web.Controllers
{
    [HandleError]
    [CompressFilter(Order = 1)]
    [CacheFilter(Order = 2)]
	public partial class RentalController : Controller
	{
		#region Private

		ILogger _Logger;
		IGeocodeService _GeocodeService;
        IRentalSearchService _RentalSearchService;
        IEmailService _EmailService;

		#endregion

        public RentalController(ILogger logger, IGeocodeService geocodeService, IRentalSearchService rentalSearchService, IEmailService emailService)
		{
			_Logger = logger;
			_GeocodeService = geocodeService;
            _RentalSearchService = rentalSearchService;
            _EmailService = emailService;
		}

		/// <summary>
		/// GET Action result to show rental details
		/// </summary>
		/// <param name="id">id of the rental</param>
		/// <returns>View containing rental details</returns>
		[HttpGet]
		[ActionName("details")]
		public virtual ActionResult Detail(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
			var rental = rRepo.Get(id);
            var rSSRVM = new RentalSearchSingleResultViewModel();
            if (rental == null)
                return View(MVC.Shared.Views.Error);
            else
                rSSRVM.Rental = rental;
			return View(rSSRVM);
		}

		/// <summary>
		/// GET action to create a new rental
		/// </summary>
		/// <returns>The form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		[ActionName("ajouter")]
		public virtual ActionResult Create()
		{
			return View(MVC.Rental.Views.editer, new RentalFormViewModel());
		}

		/// <summary>
		/// GET Action result to prepare the view to edit rental
		/// </summary>
		/// <param name="id">id of the rental</param>
		/// <returns>the form to fill</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		[ActionName("editer")]
		public virtual ActionResult Edit(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
			var rental = rRepo.Get(id);
			if (rental == null)
				return View(MVC.Shared.Views.Error);
			return View(new RentalFormViewModel(rental));
		}

        const string _RentalPrefix = "Rental";
		/// <summary>
		/// POST action to edit/create a rental
		/// create the rental if id has no value, update in database if there is an id
		/// </summary>
		/// <param name="localisation">The localisation data from the form (provided from custom model binder)</param>
		/// <param name="id">The id of the edited localisation</param>
		/// <returns>the detail view of localistion if ok, the form with errors else</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		[ActionName("editer")]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Edit(Rental rental, int? id)
		{
			var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
			var context = ModelFactory.GetUnitOfWork();
			var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
			var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
			try
			{
				var member = mRepo.GetMember(User.Identity.Name);
				if (!member.IsValidUser())
				{
					error = Worki.Resources.Validation.ValidationString.InvalidUser;
					throw new Exception(error);
				}
				if (ModelState.IsValid)
				{
					var rentalToAdd = new Rental();
					var idToRedirect = 0;
					var newRental = (!id.HasValue || id.Value == 0);
					float lat, lng;
					_GeocodeService.GeoCode(rental.FullAddress, out lat, out lng);
					if (newRental)
					{
						//update
                        UpdateModel(rentalToAdd, _RentalPrefix);
						rentalToAdd.MemberId = member.MemberId;
						rentalToAdd.CreationDate = DateTime.Now;
						rentalToAdd.Latitude = lat;
						rentalToAdd.Longitude = lng;
						//save
						rRepo.Add(rentalToAdd);
					}
					else
					{
						var r = rRepo.Get(id.Value);
                        UpdateModel(r, _RentalPrefix);
						r.TimeStamp = DateTime.Now;
						r.Latitude = lat;
						r.Longitude = lng;
					}

					context.Commit();
					idToRedirect = newRental ? rentalToAdd.Id : id.Value;
                    TempData[MiscHelpers.TempDataConstants.Info] = newRental ? Worki.Resources.Views.Rental.RentalString.RentalHaveBeenCreate  : Worki.Resources.Views.Rental.RentalString.RentalHaveBeenEdit;

					return RedirectToAction(MVC.Rental.ActionNames.Detail, new { id = idToRedirect });
				}
			}
			catch (Exception ex)
			{
				_Logger.Error("Edit", ex);
				context.Complete();
				ModelState.AddModelError("", error);
			}
			return View(new RentalFormViewModel(rental));
		}

		/// <summary>
		/// GET Action result to delete a rental
		/// if the id is in db, ask for confirmation to delete the rental
		/// </summary>
		/// <param name="id">The id of the rental to delete</param>
		/// <returns>the confirmation view</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		[ActionName("supprimer")]
		public virtual ActionResult DeleteRental(int id, string returnUrl = null)
		{
			var context = ModelFactory.GetUnitOfWork();
			var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
			var rental = rRepo.Get(id);
			if (rental == null)
				return View(MVC.Shared.Views.Error);
			else
			{
				TempData["returnUrl"] = returnUrl;
				return View(rental);
			}
		}

		/// <summary>
		/// POST Action result to delete a rental
		/// remove rental from db
		/// <param name="id">The id of the rental to delete</param>
		/// </summary>
		/// <returns>the deletetion success view</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		[ActionName("supprimer")]
		[ValidateAntiForgeryToken]
        public virtual ActionResult DeleteRental(int id, string confirm, string returnUrl)
		{
			var context = ModelFactory.GetUnitOfWork();
			var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
			try
			{
				var rental = rRepo.Get(id);
				if (rental == null)
					return View(MVC.Shared.Views.Error);
				rRepo.Delete(id);
				context.Commit();
			}
			catch (Exception ex)
			{
				_Logger.Error("Delete", ex);
				context.Complete();
			}

            TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Rental.RentalString.RentalHaveBeenDel;

            return RedirectToAction(MVC.Admin.IndexRental());
            //return Redirect(returnUrl);
		}


		public virtual PartialViewResult AddRentalAccess()
		{
			return PartialView(MVC.Rental.Views._RentalAccess, new RentalAccess());
		}

        /// <summary>
        /// GET Action result to search rentals from a RentalSearchCriteria
        /// </summary>
        /// <returns>the form to fill</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("recherche")]
        public virtual ActionResult RentalSearch()
        {
            return View(new RentalSearchCriteria());
        }

        /// <summary>
        /// POST Action result to search rentals from a RentalSearchCriteria
        /// it create the route data from RentalSearchCriteria and redirect to result page
        /// </summary>
        /// <param name="criteria">The criteria data from the form</param>
        /// <returns>redirect to the list of results</returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ActionName("recherche")]
        [ValidateAntiForgeryToken]
        [ValidateOnlyIncomingValues(Exclude = "Type,LeaseType", Prefix="RentalData")]
        public virtual ActionResult RentalSearch(RentalSearchCriteria rentalSearchCriteria)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var rvd = _RentalSearchService.GetRVD(rentalSearchCriteria);
                    return RedirectToAction(MVC.Rental.Actions.ActionNames.FullSearchResult, rvd);
                }
                catch (Exception ex)
                {
                    _Logger.Error("RentalSearch", ex);
                    ModelState.AddModelError("", Worki.Resources.Validation.ValidationString.CheckCriterias);
                }
            }
            return View(rentalSearchCriteria);
        }

        /// <summary>
        /// GET Action result to show paginated search results from a RentalSearchCriteria
        /// build searchcriteria from url
        /// with this, search matchings localisations in repository
        /// and display results
        /// </summary>
        /// <param name="page">the page to display</param>
        /// <returns>the list of results in the page</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("resultats-annonces")]
        public virtual ActionResult FullSearchResult(int? page)
        {
            var pageValue = page ?? 1;

            var criteria = _RentalSearchService.GetCriteria(Request);
            _RentalSearchService.FillSearchResults(ref criteria);
            criteria.FillPageInfo(pageValue);

            return View(criteria);
        }

        /// <summary>
        /// GET Action result to show detailed rentals from search results
        /// </summary>
        /// <param name="index">the index of th rental in the list of results</param>
        /// <returns>a view of the details of the selected rental</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("resultats-annonces-detail")]
        public virtual ActionResult FullSearchResultDetail(int? index)
        {
            var itemIndex = index ?? 0;
            var detailModel = _RentalSearchService.GetSingleResult(Request, itemIndex);

            if (detailModel == null)
                return View(MVC.Shared.Views.Error);
            return View(MVC.Rental.Views.details, detailModel);
        }

        /// <summary>
        /// GET action method to send a demand to the rental owner
        /// </summary>
        /// <param name="id">id coming from the rental's detail page.</param>
        /// <returns>The contact form to fill.</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        [ActionName("envoyer-email-propriétaire")]
        public virtual ActionResult SendMailOwner(int id)
        {
            var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
            var rental = rRepo.Get(id);
            if (rental == null)
                return View(MVC.Shared.Views.Error);
            var contact = new Contact();
            var member = mRepo.GetMember(User.Identity.Name);
            try
            {
                if (!member.IsValidUser())
                {
                    error = Worki.Resources.Validation.ValidationString.InvalidUser;
                    throw new Exception(error);
                }
                else
                {
                    contact.EMail = member.Email;
                    contact.LastName = member.MemberMainData.LastName;
                    contact.FirstName = member.MemberMainData.FirstName;
                    contact.ToEMail = rental.Member.Email;
                    contact.ToName = rental.Member.MemberMainData.LastName;
                    contact.Subject = Worki.Resources.Email.Common.Concern + rental.Reference + " - " + rental.PostalCode + " - " + rental.SurfaceString + " - " + rental.RateString;
                    contact.Link = Worki.Resources.Email.Common.LinkToDetail + id;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("Rental", ex);
                context.Complete();
            }

            return View("SendOwner", contact);
        }

        /// <summary>
        /// GET action method to send a demand to a friend
        /// </summary>
        /// <param name="id">id coming from the rental's detail page.</param>
        /// <param name="friend"></param>
        /// <returns>The contact form to fill.</returns>
        [AcceptVerbs(HttpVerbs.Get)]
        [ActionName("envoyer-email-ami")]
        public virtual ActionResult SendMailFriend(int id, string friend)
        {
            var error = Worki.Resources.Validation.ValidationString.ErrorWhenSave;
            var context = ModelFactory.GetUnitOfWork();
            var mRepo = ModelFactory.GetRepository<IMemberRepository>(context);
            var rRepo = ModelFactory.GetRepository<IRentalRepository>(context);
            var rental = rRepo.Get(id);
            if (rental == null)
                return View(MVC.Shared.Views.Error);
            var contact = new Contact();
            var member = mRepo.GetMember(User.Identity.Name);

            if (member.IsValidUser())
            {
                contact.EMail = member.Email;
                contact.LastName = member.MemberMainData.LastName;
                contact.FirstName = member.MemberMainData.FirstName;
                contact.Subject = Worki.Resources.Email.Common.Concern + rental.Reference + " - " + rental.PostalCode + " - " + rental.SurfaceString + " - " + rental.RateString;
            }
            else
                contact.LastName = User.Identity.Name;

            contact.Link = Worki.Resources.Email.Common.LinkToDetail + id;

            return View("SendFriend", contact);
        }

        /// <summary>
        /// POST action method to send a demand to contact
        /// return message sent view
        /// </summary>
        /// <param name="contact">The contact data from the form</param>
        /// <returns>message sent view if ok, the form with errors else </returns>
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateAntiForgeryToken]
        [ActionName("envoyer-email")]
        public virtual ActionResult SendMail(Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {
					dynamic contactMail = new Email(MiscHelpers.EmailConstants.EmailOwnerView);
                    contactMail.From = contact.FirstName + " " + contact.LastName + "<" + contact.EMail + ">";
                    contactMail.To = contact.ToEMail;
                    contactMail.Subject = contact.Subject;
                    contactMail.ToName = contact.ToName;
                    contactMail.Content = contact.Message;
                    contactMail.Link = contact.Link;
                    contactMail.Send();
                }
                catch (Exception ex)
                {
                    _Logger.Error("Rental", ex);
                }
                TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Home.HomeString.MailWellSent2;
                return Redirect(contact.Link);
            }
            return View(contact);
        }
	}
}
