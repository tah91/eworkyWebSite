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
using System.Collections.Generic;

namespace Worki.Web.Controllers
{
	public partial class OfferController : ControllerBase
	{
		#region Private

        IMembershipService _MembershipService;

		#endregion

        public OfferController( ILogger logger, 
                                IObjectStore objectStore,
                                IMembershipService membershipService)
            : base(logger, objectStore)
		{
            _MembershipService = membershipService;
		}

        /// <summary>
        /// GET Action result to show offer form
        /// </summary>
        /// <param name="id">id of the offer localisation</param>
        /// <returns>View containing offer form</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        public virtual ActionResult Create(int id, int type, string returnUrl = null)
        {
            throw new NotImplementedException("Deprecated");
        }

		/// <summary>
		/// Post Action result to add offer
		/// </summary>
		/// <returns>View containing localisation form</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
		[ValidateAntiForgeryToken]
		public virtual ActionResult Create(int id, string returnUrl, OfferFormViewModel offerFormViewModel)
        {
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Offer), new PictureDataContainer(offerFormViewModel.Offer));

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
                        _ObjectStore.Delete(PictureData.GetKey(ProviderType.Offer));
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }
					TempData[MiscHelpers.TempDataConstants.Info] = Worki.Resources.Views.Offer.OfferString.OfferCreated;
					if (!string.IsNullOrEmpty(returnUrl))
					{
						return Redirect(returnUrl);
					}
					else
					{
						return RedirectToAction(MVC.Localisation.Edit(offerFormViewModel.Offer.LocalisationId));
					}
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
			return View(MVC.Offer.Views.Create, new OfferFormViewModel(offer.Localisation.IsSharedOffice()) { Offer = offer });
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
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Offer), new PictureDataContainer(formData.Offer));

			var context = ModelFactory.GetUnitOfWork();
			var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
			if (ModelState.IsValid)
			{
				try
				{
					var o = oRepo.Get(id);
					UpdateModel(o, "Offer");
					context.Commit();
                    _ObjectStore.Delete(PictureData.GetKey(ProviderType.Offer));
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
		public virtual PartialViewResult AddOfferPrice(int priceType)
		{
            return PartialView(MVC.Offer.Views._OfferPrice, new OfferPrice { PriceType = priceType });
		}

		#region Ajax Offer

		/// <summary>
		/// POST Action result add an offer via ajax
		/// </summary>
		/// <param name="id">localisation id</param>
        /// <param name="offerFormViewModel">offer data</param>
		/// <returns>exception if error, partial view if added</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
        //[ValidateAntiForgeryToken]
        [HandleModelStateException]
        public virtual ActionResult AjaxAdd(int id, OfferFormViewModel offerFormViewModel)
        {
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Offer), new PictureDataContainer(offerFormViewModel.Offer));

            if (ModelState.IsValid)
            {
                try
                {
                    var context = ModelFactory.GetUnitOfWork();
                    var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(context);
                    var loc = lRepo.Get(id);
                    var offerType = (LocalisationOffer)offerFormViewModel.Offer.Type;

                    try
                    {
                        loc.Offers.Add(offerFormViewModel.Offer);
                        if (offerFormViewModel.DuplicateCount > 0)
                        {
                            var toAdd = offerFormViewModel.Offer.Replicate(offerFormViewModel.DuplicateCount);
                            foreach (var offer in toAdd)
                            {
                                loc.Offers.Add(offer);
                            }
                        }
                        //force online when one offer added
                        loc.MainLocalisation.IsOffline = false;
                        context.Commit();
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }

                    _ObjectStore.Delete(PictureData.GetKey(ProviderType.Offer));

                    var newContext = ModelFactory.GetUnitOfWork();
                    lRepo = ModelFactory.GetRepository<ILocalisationRepository>(newContext);
                    loc = lRepo.Get(id);
                    var offerCountModel = new OfferCounterModel(loc);
                    int currentNeed;
                    string helpText;
                    LocalisationOffer offerTypeToAdd;
                    var newList = this.RenderRazorViewToString(MVC.Offer.Views._OfferList, offerCountModel);

                    if (offerCountModel.NeedAddThisOffer(offerType, out currentNeed, out helpText))
                    {
                        var newForm = this.RenderRazorViewToString(MVC.Offer.Views._AjaxAdd, new OfferFormViewModel(loc.IsSharedOffice(), offerType, currentNeed) { LocId = id });

                        return Json(new 
                        { 
                            help = helpText, 
                            form = newForm, 
                            newList = newList,
                            newBounds = offerCountModel.GetJson()
                        });
                    }
                    else if (offerCountModel.NeedAddOffer(out offerTypeToAdd, out currentNeed, out helpText))
                    {
                        var newForm = this.RenderRazorViewToString(MVC.Offer.Views._AjaxAdd, new OfferFormViewModel(loc.IsSharedOffice(), offerTypeToAdd, currentNeed) { LocId = id });
                        return Json(new
                        {
                            help = helpText,
                            form = newForm,
                            newList = newList,
                            newBounds = offerCountModel.GetJson()
                        });
                    }
                    else
                    {
                        return Json(new
                        {
                            help = "",
                            form = "",
                            newList = newList,
                            newBounds = offerCountModel.GetJson()
                        });
                    }
                }
                catch (Exception ex)
                {
                    _Logger.Error("AjaxAdd", ex);
                    ModelState.AddModelError("", ex.Message);
                    throw new ModelStateException(ModelState);
                }
            }
            throw new ModelStateException(ModelState);
        }

        /// <summary>
        /// Action result to return offer edition form
        /// </summary>
        /// <param name="id">id of offer if any</param>
        /// <returns>a partial view</returns>
        [AcceptVerbs(HttpVerbs.Get), Authorize]
        [HandleModelStateException]
        public virtual ActionResult AjaxEdit(int id, bool isShared)
        {
            var context = ModelFactory.GetUnitOfWork();
            var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
            var offer = oRepo.Get(id);

            var editForm = this.RenderRazorViewToString(MVC.Offer.Views._AjaxAdd, new OfferFormViewModel(isShared) { Offer = offer });
            return Json(new { help = "", form = editForm }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// POST Action result edit an offer via ajax
        /// </summary>
        /// <param name="id">offer id</param>
        /// <param name="offerFormViewModel">offer data</param>
        /// <returns>exception if error, partial view if added</returns>
        [AcceptVerbs(HttpVerbs.Post), Authorize]
        //[ValidateAntiForgeryToken]
        [HandleModelStateException]
        public virtual ActionResult AjaxEdit(int id, OfferFormViewModel offerFormViewModel)
        {
            _ObjectStore.Store<PictureDataContainer>(PictureData.GetKey(ProviderType.Offer), new PictureDataContainer(offerFormViewModel.Offer));

            if (ModelState.IsValid)
            {
                try
                {
                    var context = ModelFactory.GetUnitOfWork();
                    var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
                    var offer = oRepo.Get(id);
                    try
                    {
                        var locId = offer.LocalisationId;
                        UpdateModel(offer, "Offer");
                        context.Commit();

                        _ObjectStore.Delete(PictureData.GetKey(ProviderType.Offer));

                        var newContext = ModelFactory.GetUnitOfWork();
                        var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(newContext);
                        var loc = lRepo.Get(locId);
                        var offerCountModel = new OfferCounterModel(loc);
                        var newList = this.RenderRazorViewToString(MVC.Offer.Views._OfferList, offerCountModel);
                        
                        return Json(new { help = "", form = "", newList = newList });
                    }
                    catch (Exception ex)
                    {
                        _Logger.Error(ex.Message);
                        context.Complete();
                        throw ex;
                    }
                }
                catch (Exception ex)
                {
                    _Logger.Error("AjaxEdit", ex);
                    ModelState.AddModelError("", ex.Message);
                    throw new ModelStateException(ModelState);
                }
            }
            throw new ModelStateException(ModelState);
        }

        /// <summary>
        /// Action result to delete offer
        /// </summary>
        /// <param name="id">id of offer if any</param>
        /// <returns>a partial view</returns>
        [HandleModelStateException]
        public virtual ActionResult AjaxDelete(int id)
        {
            try
            {
                var context = ModelFactory.GetUnitOfWork();
                var oRepo = ModelFactory.GetRepository<IOfferRepository>(context);
                try
                {
                    var offer = oRepo.Get(id);
                    var locId = offer.LocalisationId;
                    var isShared = offer.Localisation.IsSharedOffice();
                    var removedType = (LocalisationOffer)offer.Type;
                    oRepo.Delete(id);
                    context.Commit();

                    var newContext = ModelFactory.GetUnitOfWork();
                    var lRepo = ModelFactory.GetRepository<ILocalisationRepository>(newContext);
                    var loc = lRepo.Get(locId);
                    var offerCountModel = new OfferCounterModel(loc);
                    var newList = this.RenderRazorViewToString(MVC.Offer.Views._OfferList, offerCountModel);

                    return Json(new 
                    {
                        help = "",
                        form = "", 
                        newList = newList,
                        newBounds = offerCountModel.GetJson(),
                        toDecrement = removedType.ToString()
                    });
                }
                catch (Exception ex)
                {
                    _Logger.Error("AjaxDelete", ex);
                    context.Complete();
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error("AjaxDelete", ex);
                ModelState.AddModelError("", ex.Message);
                throw new ModelStateException(ModelState);
            }
        }

		#endregion
	}
}
