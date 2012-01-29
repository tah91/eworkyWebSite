using System;
using System.Web.Mvc;
using Worki.Data.Models;
using Worki.Data.Repository;
using Worki.Infrastructure.Helpers;
using Worki.Infrastructure.Logging;
using Worki.Infrastructure.Repository;
using Worki.Web.Helpers;
using Worki.Service;
using Worki.Infrastructure;
using Postal;
using System.Linq;
using Worki.Memberships;
using System.Web.Security;

namespace Worki.Web.Controllers
{
	public partial class OfferController : ControllerBase
	{
		#region Private

		ILogger _Logger;

		#endregion

		public OfferController(ILogger logger)
		{
			_Logger = logger;
		}

		/// <summary>
		/// GET Action result to show offer form
		/// </summary>
		/// <param name="id">id of the offer localisation</param>
		/// <returns>View containing offer form</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		public virtual ActionResult Create(int id, int type)
		{
            var context = ModelFactory.GetUnitOfWork();
            var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
            var loc = lRepo.Get(id);

			return View(new OfferFormViewModel(loc.IsSharedOffice(), Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminRole)) { Offer = new Offer { LocalisationId = id, Type = type } });
		}

		/// <summary>
		/// Post Action result to add offer
		/// </summary>
		/// <returns>View containing localisation form</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Create(int id, OfferFormViewModel offerFormViewModel)
        {
            TempData[PictureData.PictureDataString] = new PictureDataContainer(offerFormViewModel.Offer);
            if (ModelState.IsValid)
            {
                try
                {
                    var context = ModelFactory.GetUnitOfWork();
                    var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
					var loc = lRepo.Get(id);
                    try
                    {
						loc.Offers.Add(offerFormViewModel.Offer);
                        context.Commit();
						TempData.Remove(PictureData.PictureDataString);
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferCreated;
					return RedirectToAction(MVC.Localisation.Edit(offerFormViewModel.Offer.LocalisationId));
                }
                catch (Exception ex)
                {
                    _Logger.Error("Create", ex);
                    ModelState.AddModelError("", ex.Message);
                }
            }
			return View(offerFormViewModel);
        }

		/// <summary>
		/// GET Action result to show offer data
		/// </summary>
		/// <param name="id">id of offer</param>
		/// <returns>View containing offer data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize(Roles = MiscHelpers.AdminConstants.AdminRole)]
		public virtual ActionResult Details(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);
			return View(offer);
		}

		/// <summary>
		/// GET Action result to edit offer data
		/// </summary>
		/// <param name="id">id of offer</param>
		/// <returns>View containing offer data</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		public virtual ActionResult Edit(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);
			return View(MVC.Offer.Views.Create, new OfferFormViewModel(offer.Localisation.IsSharedOffice(), Roles.IsUserInRole(MiscHelpers.AdminConstants.AdminRole)) { Offer = offer });
		}

		/// <summary>
		/// Post Action result to edit offer data
		/// </summary>
		/// <param name="id">id of offer</param>
		/// <returns>View containing localisation data</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Edit(int id, OfferFormViewModel formData)
		{
			var context = ModelFactory.GetUnitOfWork();
            TempData[PictureData.PictureDataString] = new PictureDataContainer(formData.Offer);
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			if (ModelState.IsValid)
			{
				try
				{
					var o = oRepo.Get(id);
					UpdateModel(o, "Offer");
					context.Commit();
					TempData.Remove(PictureData.PictureDataString);
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferEdited;
					return RedirectToAction(MVC.Localisation.Edit(o.LocalisationId));
				}
				catch (Exception ex)
				{
					_Logger.Error("Edit", ex);
					context.Complete();
					ModelState.AddModelError("", ex.Message);
				}
			}
			return View(MVC.Offer.Views.Create, formData);
		}

		/// <summary>
		/// GET Action result to delete an offer
		/// if the id is in db, ask for confirmation to delete the offer
		/// </summary>
		/// <param name="id">The id of the offer to delete</param>
		/// <returns>the confirmation view</returns>
		[AcceptVerbs(HttpVerbs.Get), Authorize]
		//[ActionName("supprimer")]
		public virtual ActionResult Delete(int id, string returnUrl = null)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			var offer = oRepo.Get(id);
			if (offer == null)
				return View(MVC.Shared.Views.Error);
			else
			{
				TempData["returnUrl"] = returnUrl;
				return View(offer);
			}
		}

		/// <summary>
		/// POST Action result to delete an offer
		/// remove offer from db
		/// <param name="id">The id of the offer to delete</param>
		/// </summary>
		/// <returns>the deletetion success view</returns>
		[AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Delete(int id)
		{
			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			int localisationId = 0;
			try
			{
				var o = oRepo.Get(id);
				localisationId = o.LocalisationId;
				oRepo.Delete(id);
				context.Commit();
			}
			catch (Exception ex)
			{
				_Logger.Error("Delete", ex);
				context.Complete();
			}

			TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferRemoved;
			return RedirectToAction(MVC.Localisation.Edit(localisationId));
		}

		/// <summary>
		/// Action result to return offerprice item for edition
		/// </summary>
		/// <returns>a partial view</returns>
		public virtual PartialViewResult AddOfferPrice()
		{
			return PartialView(MVC.Offer.Views._OfferPrice, new OfferPrice());
		}
    }
}
