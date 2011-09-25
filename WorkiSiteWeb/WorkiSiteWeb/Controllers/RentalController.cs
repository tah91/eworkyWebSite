using System;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Infrastructure;
using Worki.Infrastructure.Logging;
using Worki.Service;
using Worki.Infrastructure.Repository;

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

		#endregion

        public RentalController(ILogger logger,IGeocodeService geocodeService)
		{
			_Logger = logger;
			_GeocodeService = geocodeService;
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
			if (rental == null)
				return View(MVC.Shared.Views.Error);

			return View(rental);
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
						UpdateModel(rentalToAdd, "Rental");
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
						UpdateModel(r);
						r.TimeStamp = DateTime.Now;
						r.Latitude = lat;
						r.Longitude = lng;
					}

					context.Commit();
					idToRedirect = newRental ? rentalToAdd.Id : id.Value;
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
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		[ActionName("supprimer")]
		public virtual ActionResult Delete(int id, string returnUrl = null)
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
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		[ActionName("supprimer")]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Delete(int id, string confirm, string returnUrl)
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

			if (string.IsNullOrEmpty(returnUrl))
				return View(MVC.Localisation.Views.supprimer_reussi);
			else
				return Redirect(returnUrl);
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
        public virtual ActionResult RentalSearch()
        {
            //return View(new RentalSearchCriteria());
            return View("RentalSearch");
        }
	}
}
